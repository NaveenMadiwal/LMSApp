using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using LMSApp.Models.ViewModels;
using LMSApp.Services;

namespace LMSApp.Controllers
{
    [Authorize(Roles = "Instructor")]
    public class InstructorController : Controller
    {
        private readonly ICourseService _courseService;
        private readonly IEnrollmentService _enrollmentService;

        public InstructorController(ICourseService courseService, IEnrollmentService enrollmentService)
        {
            _courseService = courseService;
            _enrollmentService = enrollmentService;
        }

        public async Task<IActionResult> Dashboard()
        {
            var instructorId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(instructorId))
            {
                TempData["Error"] = "User not authenticated";
                return RedirectToAction("Login", "Account", new { area = "Identity" });
            }

            // Get all courses created by this instructor
            var courses = await _courseService.GetCoursesByCreatorAsync(instructorId);
            var courseIds = courses.Select(c => c.Id).ToList();
            int courseCount = courses.Count();

            // Get all enrollments for these courses
            var allEnrollments = new List<EnrollmentViewModel>();
            foreach (var courseId in courseIds)
            {
                var enrollments = await _enrollmentService.GetEnrollmentsByCourseAsync(courseId, instructorId);
                allEnrollments.AddRange(enrollments);
            }
            int enrolledStudentCount = allEnrollments.Select(e => e.StudentId).Distinct().Count();

            var dashboardVM = new InstructorDashboardViewModel
            {
                CourseCount = courseCount,
                EnrolledStudentCount = enrolledStudentCount,
                RecentCourses = courses.OrderByDescending(c => c.CreatedAt).Take(3).ToList(),
                RecentEnrollments = allEnrollments.OrderByDescending(e => e.EnrolledOn).Take(3).ToList()
            };
            return View(dashboardVM);
        }

        public async Task<IActionResult> Courses()
        {
            try
            {
                var instructorId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(instructorId))
                {
                    TempData["Error"] = "User not authenticated";
                    return RedirectToAction("Dashboard");
                }

                System.Diagnostics.Debug.WriteLine($"Loading courses page for instructor ID: {instructorId}");
                
                var courses = await _courseService.GetCoursesByCreatorAsync(instructorId);
                
                System.Diagnostics.Debug.WriteLine($"Found {courses.Count()} courses for instructor {instructorId}");
                
                return View(courses);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Exception in Courses action: {ex.Message}");
                TempData["Error"] = "Error loading courses";
                return View(new List<CourseListViewModel>());
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetMyCourses()
        {
            try
            {
                var instructorId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(instructorId))
                {
                    return Json(new { success = false, message = "User not authenticated" });
                }

                System.Diagnostics.Debug.WriteLine($"Getting courses for instructor ID: {instructorId}");
                
                var courses = await _courseService.GetCoursesByCreatorAsync(instructorId);
                
                System.Diagnostics.Debug.WriteLine($"Found {courses.Count()} courses for instructor {instructorId}");
                
                return Json(new { success = true, data = courses });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Exception: {ex.Message}");
                return Json(new { success = false, message = "Error loading courses", error = ex.Message });
            }
        }

        public IActionResult Enrollments()
        {
            return View();
        }
         
        [HttpGet("Instructor/GetCourseEnrollments/{courseId}")]
        public async Task<IActionResult> GetCourseEnrollments(int courseId)
        {
            try
            {
                var instructorId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(instructorId))
                {
                    return Json(new { success = false, message = "User not authenticated" });
                }

                // Get course details to verify ownership and get title
                var course = await _courseService.GetCourseAsync(courseId);
                if (course == null)
                {
                    return Json(new { success = false, message = "Course not found" });
                }

                if (course.CreatedById != instructorId)
                {
                    return Json(new { success = false, message = "Access denied to this course" });
                }

                // Get enrollments for this course
                var enrollments = await _enrollmentService.GetEnrollmentsByCourseAsync(courseId, instructorId);
                
                return Json(new { 
                    success = true, 
                    data = enrollments,
                    courseTitle = course.Title
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error loading enrollments", error = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var instructorId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(instructorId))
            {
                TempData["Error"] = "User not authenticated";
                return RedirectToAction("Login", "Account", new { area = "Identity" });
            }

            // Load instructor info
            var user = await _courseService.GetUserByIdAsync(instructorId); // We'll add this method
            if (user == null)
            {
                TempData["Error"] = "User not found.";
                return RedirectToAction("Dashboard");
            }

            var vm = new ProfileViewModel
            {
                FullName = user.FullName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                DateOfBirth = user.DateOfBirth,
                RoleType = user.RoleType ?? "Instructor",
                IsActive = user.IsActive,
                LastLogin = user.LockoutEnd?.UtcDateTime,
            };
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Profile(ProfileViewModel model)
        {
            //if (!ModelState.IsValid)
            //{
            //    TempData["Error"] = "Please correct the errors and try again.";
            //    return View(model);
            //}

            var instructorId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(instructorId))
            {
                TempData["Error"] = "User not authenticated";
                return RedirectToAction("Login", "Account", new { area = "Identity" });
            }

            // Update user
            var result = await _courseService.UpdateUserProfileAsync(instructorId, model); // We'll add this method
            if (result)
            {
                TempData["Success"] = "Profile updated successfully.";
                return RedirectToAction("Profile");
            }
            else
            {
                TempData["Error"] = "Failed to update profile. Please try again.";
                return View(model);
            }
        }

        // Course Management Actions
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateCourse(CourseViewModel model)
        {
            // Debug: Log the received model and ModelState
            var modelStateErrors = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .ToList();
            
            var receivedData = new
            {
                Title = model?.Title,
                Description = model?.Description,
                CategoryId = model?.CategoryId,
                StartDate = model?.StartDate,
                EndDate = model?.EndDate,
                IsActive = model?.IsActive
            };
            
            if (!ModelState.IsValid)
            {
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { 
                        success = false, 
                        message = "Invalid course data.", 
                        errors = modelStateErrors,
                        receivedData = receivedData
                    });
                }
                TempData["Error"] = "Invalid course data: " + string.Join(", ", modelStateErrors);
                return RedirectToAction("Courses");
            }

            // Set the CreatedById to the current user's ID
            model.CreatedById = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            var success = await _courseService.CreateCourseAsync(model);

            if (success)
            {
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = true, message = "Course created successfully." });
                }
                TempData["Success"] = "Course created successfully.";
            }
            else
            {
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = false, message = "Failed to create course." });
                }
                TempData["Error"] = "Failed to create course.";
            }

            return RedirectToAction("Courses");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateCourse(CourseViewModel model)
        {
            if (!ModelState.IsValid)
            {
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = false, message = "Invalid course data." });
                }
                TempData["Error"] = "Invalid course data.";
                return RedirectToAction("Courses");
            }

            // Set the CreatedById to the current user's ID
            model.CreatedById = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            var success = await _courseService.UpdateCourseAsync(model.Id, model);

            if (success)
            {
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = true, message = "Course updated successfully.", courseTitle = model.Title });
                }
                TempData["Success"] = "Course updated successfully.";
            }
            else
            {
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = false, message = "Failed to update course." });
                }
                TempData["Error"] = "Failed to update course.";
            }

            return RedirectToAction("Courses");
        }

        [HttpPost]
        public async Task<IActionResult> DeleteCourse(int id)
        {
            var instructorId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var success = await _courseService.DeleteCourseAsync(id, instructorId);

            if (success)
            {
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = true, message = "Course deleted successfully." });
                }
                TempData["Success"] = "Course deleted successfully.";
            }
            else
            {
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = false, message = "Failed to delete course." });
                }
                TempData["Error"] = "Failed to delete course.";
            }

            return RedirectToAction("Courses");
        }
    }
}
