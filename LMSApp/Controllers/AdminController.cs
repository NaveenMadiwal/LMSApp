using LMSApp.Data;
using LMSApp.Models;
using LMSApp.Models.ViewModels;
using LMSApp.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LMSApp.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly IAdminService _adminService;

        public AdminController(IAdminService adminService)
        {
            _adminService = adminService;
        }
        public async Task<IActionResult> Dashboard()
        {
            try
            {
                var model = await _adminService.GetDashboardStatisticsAsync();
                return View(model);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error loading dashboard statistics";
                return View(new AdminDashboardViewModel());
            }
        }

        public async Task<IActionResult> Category()
        {
            try
            {
                var categories = await _adminService.GetCategoriesAsync();
                return View(categories);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error loading categories";
                return View(new List<Category>());
            }
        }

        public async Task<IActionResult> Course()
        {
            try
            {
                var courses = await _adminService.GetCoursesAsync();
                return View(courses);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error loading courses";
                return View(new List<CourseListViewModel>());
            }
        }

        public async Task<IActionResult> Student()
        {
            try
            {
                var students = await _adminService.GetStudentsAsync();
                return View(students);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error loading students";
                return View(new List<StudentListViewModel>());
            }
        }

        public async Task<IActionResult> StudentEnrollments()
        {
            try
            {
                var students = await _adminService.GetStudentsWithEnrollmentsAsync();
                return View(students);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error loading student enrollments";
                return View(new List<StudentListViewModel>());
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateCategory(CategoryViewModel model)
        {
            if (!ModelState.IsValid)
            {
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = false, message = "Invalid category name." });
                }
                TempData["Error"] = "Invalid category name.";
                return RedirectToAction("Category");
            }

            try
            {
                var success = await _adminService.CreateCategoryAsync(model);

                if (success)
                {
                    if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    {
                        return Json(new { success = true, message = "Category added successfully." });
                    }
                    TempData["Success"] = "Category added successfully.";
                }
                else
                {
                    if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    {
                        return Json(new { success = false, message = "Failed to add category." });
                    }
                    TempData["Error"] = "Failed to add category.";
                }
            }
            catch (Exception ex)
            {
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = false, message = "Error creating category." });
                }
                TempData["Error"] = "Error creating category.";
            }

            return RedirectToAction("Category");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateCategory(CategoryViewModel model)
        {
            if (!ModelState.IsValid)
            {
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = false, message = "Invalid data." });
                }
                TempData["Error"] = "Invalid data.";
                return RedirectToAction("Category");
            }

            try
            {
                var success = await _adminService.UpdateCategoryAsync(model);

                if (success)
                {
                    if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    {
                        return Json(new { success = true, message = "Category updated successfully.", categoryName = model.Name });
                    }
                    TempData["Success"] = "Category updated successfully.";
                }
                else
                {
                    if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    {
                        return Json(new { success = false, message = "Failed to update category." });
                    }
                    TempData["Error"] = "Failed to update category.";
                }
            }
            catch (Exception ex)
            {
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = false, message = "Error updating category." });
                }
                TempData["Error"] = "Error updating category.";
            }

            return RedirectToAction("Category");
        }

        [HttpPost]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            var success = await _adminService.DeleteCategoryAsync(id);

            if (success)
            {
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = true, message = "Category deleted successfully." });
                }
                TempData["Success"] = "Category deleted (inactivated).";
            }
            else
            {
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = false, message = "Failed to delete category." });
                }
                TempData["Error"] = "Failed to delete category.";
            }

            return RedirectToAction("Category");
        }

        // Course Management Actions
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateCourse(CourseViewModel model)
        {
            if (!ModelState.IsValid)
            {
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = false, message = "Invalid course data." });
                }
                TempData["Error"] = "Invalid course data.";
                return RedirectToAction("Course");
            }

            // Set the CreatedById to the current user's ID if not provided
            if (string.IsNullOrEmpty(model.CreatedById))
            {
                model.CreatedById = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            }

            var success = await _adminService.CreateCourseAsync(model);

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

            return RedirectToAction("Course");
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
                return RedirectToAction("Course");
            }

            // Set the CreatedById to the current user's ID if not provided
            if (string.IsNullOrEmpty(model.CreatedById))
            {
                model.CreatedById = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            }

            var success = await _adminService.UpdateCourseAsync(model);

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

            return RedirectToAction("Course");
        }

        [HttpPost]
        public async Task<IActionResult> DeleteCourse(int id)
        {
            var success = await _adminService.DeleteCourseAsync(id);

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

            return RedirectToAction("Course");
        }

        // Student Management Actions
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateStudent(StudentViewModel model)
        {
            if (!ModelState.IsValid)
            {
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = false, message = "Invalid user data." });
                }
                TempData["Error"] = "Invalid user data.";
                return RedirectToAction("Student");
            }

            var success = await _adminService.CreateStudentAsync(model);

            if (success)
            {
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = true, message = "User created successfully." });
                }
                TempData["Success"] = "User created successfully.";
            }
            else
            {
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = false, message = "Failed to create user." });
                }
                TempData["Error"] = "Failed to create user.";
            }

            return RedirectToAction("Student");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStudent(StudentViewModel model)
        {
            if (!ModelState.IsValid)
            {
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = false, message = "Invalid user data." });
                }
                TempData["Error"] = "Invalid user data.";
                return RedirectToAction("Student");
            }

            var success = await _adminService.UpdateStudentAsync(model);

            if (success)
            {
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = true, message = "User updated successfully." });
                }
                TempData["Success"] = "User updated successfully.";
            }
            else
            {
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = false, message = "Failed to update user." });
                }
                TempData["Error"] = "Failed to update user.";
            }

            return RedirectToAction("Student");
        }

        [HttpPost]
        public async Task<IActionResult> DeleteStudent(string id)
        {
            var success = await _adminService.DeleteStudentAsync(id);

            if (success)
            {
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = true, message = "User deleted successfully." });
                }
                TempData["Success"] = "User deleted successfully.";
            }
            else
            {
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = false, message = "Failed to delete user." });
                }
                TempData["Error"] = "Failed to delete user.";
            }

            return RedirectToAction("Student");
        }

        [HttpPost]
        public async Task<IActionResult> ToggleUserActive([FromBody] ToggleUserActiveRequest request)
        {
            if (string.IsNullOrEmpty(request.userId))
            {
                return Json(new { success = false, message = "Invalid user id." });
            }
            var model = new LMSApp.Models.ViewModels.StudentViewModel
            {
                Id = request.userId,
                IsActive = request.isActive
            };
            // Only update IsActive, so skip validation for other fields
            var result = await _adminService.UpdateUserActiveOrInactive(model);
            if (result)
            {
                return Json(new { success = true });
            }
            else
            {
                return Json(new { success = false, message = "Failed to update user status." });
            }
        }

        public class ToggleUserActiveRequest
        {
            public string userId { get; set; }
            public bool isActive { get; set; }
        }

        // Feedback Management
        public async Task<IActionResult> Feedback()
        {
            try
            {
                var feedback = await _adminService.GetFeedbackAsync();
                return View(feedback);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error loading feedback";
                return View(new List<FeedbackViewModel>());
            }
        }

        public async Task<IActionResult> FeedbackAnalytics()
        {
            try
            {
                var analytics = await _adminService.GetFeedbackAnalyticsAsync();
                return View(analytics);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error loading feedback analytics";
                return View(new FeedbackAnalyticsViewModel());
            }
        }
         
        [HttpGet("Admin/GetFeedbackDetails/{feedbackId}")]
        public async Task<IActionResult> GetFeedbackDetails(int feedbackId)
         {
            var feedback = await _adminService.GetFeedbackByIdAsync(feedbackId);
            if (feedback == null)
            {
                return Content("Feedback not found");
            }
            return PartialView("_FeedbackDetails", feedback);
        }

        // Reports
        public async Task<IActionResult> Reports()
        {
            try
            {
                var reports = await _adminService.GetReportsAsync();
                return View(reports);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error loading reports";
                return View(new AdminReportsViewModel());
            }
        }

        [HttpGet]
        public async Task<IActionResult> ExportReport()
        {
            var reports = await _adminService.GetReportsAsync();
            var sb = new System.Text.StringBuilder();
            sb.AppendLine("Section,Title,Value");
            sb.AppendLine($"Enrollments,Total Enrollments,{reports.EnrollmentReport.TotalEnrollments}");
            sb.AppendLine($"Enrollments,Active Enrollments,{reports.EnrollmentReport.ActiveEnrollments}");
            sb.AppendLine($"Enrollments,Completed Enrollments,{reports.EnrollmentReport.CompletedEnrollments}");
            sb.AppendLine($"Enrollments,Completion Rate,{reports.EnrollmentReport.CompletionRate:F1}%");
            sb.AppendLine($"Courses,Total Courses,{reports.CourseReport.TotalCourses}");
            sb.AppendLine($"Courses,Active Courses,{reports.CourseReport.ActiveCourses}");
            sb.AppendLine($"Courses,Inactive Courses,{reports.CourseReport.InactiveCourses}");
            sb.AppendLine($"Courses,Average Course Rating,{reports.CourseReport.AverageCourseRating:F1}");
            foreach (var course in reports.CourseReport.HighestRatedCourses)
            {
                sb.AppendLine($"Top Rated Course,{course.CourseTitle} (by {course.InstructorName}),{course.AverageRating:F1}/5");
            }
            foreach (var course in reports.CourseReport.MostPopularCourses)
            {
                sb.AppendLine($"Popular Course,{course.CourseTitle} (by {course.InstructorName}),{course.EnrollmentCount} enrollments");
            }
            foreach (var user in reports.UserReport.MostActiveUsers)
            {
                sb.AppendLine($"Active Student,{user.UserName} ({user.Email}),{user.EnrolledCourses} enrolled, {user.CompletedCourses} completed");
            }
            var bytes = System.Text.Encoding.UTF8.GetBytes(sb.ToString());
            return File(bytes, "text/csv", $"LMS_Report_{DateTime.Now:yyyyMMdd_HHmmss}.csv");
        }

        // Activity Logs
        public async Task<IActionResult> ActivityLogs()
        {
            try
            {
                var activities = await _adminService.GetRecentActivityAsync();
                return View(activities);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error loading activity logs";
                return View(new List<ActivityLogViewModel>());
            }
        }
    }
}
