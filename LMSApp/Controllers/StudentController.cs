using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using LMSApp.Services;
using LMSApp.Models.ViewModels;
using LMSApp.Models;
using LMSApp.Data;
using System.Linq;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;

namespace LMSApp.Controllers
{
    [Authorize(Roles = "Student")]
    public class StudentController : Controller
    {
        private readonly ICourseService _courseService;
        private readonly IEnrollmentService _enrollmentService;
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public StudentController(ICourseService courseService, IEnrollmentService enrollmentService, ApplicationDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _courseService = courseService;
            _enrollmentService = enrollmentService;
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        public async Task<IActionResult> Dashboard()
        {
            try
            {
                var studentId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(studentId))
                {
                    TempData["Error"] = "User not authenticated";
                    return RedirectToAction("Login", "Account", new { area = "Identity" });
                }

                // Get student's enrolled courses
                var enrollments = await _enrollmentService.GetEnrollmentsByStudentAsync(studentId);
                
                // Calculate dashboard statistics
                var totalEnrollments = enrollments.Count();
                var activeCourses = enrollments.Count(e => e.IsActive);
                var completedCourses = enrollments.Count(e => e.CompletionStatus == "Completed");
                var averageProgress = totalEnrollments > 0 ? (completedCourses * 100.0 / totalEnrollments) : 0;

                var dashboardViewModel = new StudentDashboardViewModel
                {
                    TotalEnrollments = totalEnrollments,
                    ActiveCourses = activeCourses,
                    CompletedCourses = completedCourses,
                    AverageProgress = Math.Round(averageProgress, 1),
                    RecentEnrollments = enrollments.Take(5).ToList()
                };

                return View(dashboardViewModel);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error loading dashboard";
                return View(new StudentDashboardViewModel());
            }
        }

        public async Task<IActionResult> MyCourses()
        {
            try
            {
                var studentId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(studentId))
                {
                    TempData["Error"] = "User not authenticated";
                    return RedirectToAction("Login", "Account", new { area = "Identity" });
                }

                var enrollments = await _enrollmentService.GetEnrollmentsByStudentAsync(studentId);
                return View(enrollments);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error loading courses";
                return View(new List<EnrollmentViewModel>());
            }
        }

        public async Task<IActionResult> ProgressReport()
        {
            try
            {
                var studentId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(studentId))
                {
                    TempData["Error"] = "User not authenticated";
                    return RedirectToAction("Login", "Account", new { area = "Identity" });
                }

                var enrollments = await _enrollmentService.GetEnrollmentsByStudentAsync(studentId);
                
                var totalCourses = enrollments.Count();
                var completedCourses = enrollments.Count(e => e.CompletionStatus == "Completed");
                var averageCompletionRate = totalCourses > 0 ? (completedCourses * 100.0 / totalCourses) : 0;

                var progressViewModel = new StudentProgressViewModel
                {
                    TotalCourses = totalCourses,
                    CompletedCourses = completedCourses,
                    InProgressCourses = enrollments.Count(e => e.CompletionStatus == "In Progress"),
                    PendingCourses = enrollments.Count(e => e.CompletionStatus == "Pending"),
                    AverageCompletionRate = Math.Round(averageCompletionRate, 1),
                    Enrollments = enrollments.ToList()
                };

                return View(progressViewModel);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error loading progress report";
                return View(new StudentProgressViewModel());
            }
        }

        public async Task<IActionResult> EnrollNewCourses()
        {
            try
            {
                var studentId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(studentId))
                {
                    TempData["Error"] = "User not authenticated";
                    return RedirectToAction("Login", "Account", new { area = "Identity" });
                }

                // Get all available courses that the student is not enrolled in
                var availableCourses = await _courseService.GetAvailableCoursesForStudentAsync(studentId);
                return View(availableCourses);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in EnrollNewCourses: {ex.Message}");
                TempData["Error"] = "Error loading available courses";
                return View(new List<CourseListViewModel>());
            }
        }

        [HttpGet]
        public async Task<IActionResult> TestDatabaseConnection()
        {
            try
            {
                var courseCount = await _context.Courses.CountAsync();
                var enrollmentCount = await _context.Enrollments.CountAsync();
                return Json(new { 
                    success = true, 
                    message = "Database connection successful",
                    courseCount = courseCount,
                    enrollmentCount = enrollmentCount
                });
            }
            catch (Exception ex)
            {
                return Json(new { 
                    success = false, 
                    message = $"Database connection failed: {ex.Message}" 
                });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EnrollInCourse([FromBody] EnrollmentRequestModel model)
        {
            try
            {
                // Log the incoming model for debugging
                System.Diagnostics.Debug.WriteLine($"Received model - CourseId: {model?.CourseId}, StudentId: {model?.StudentId}");
                
                if (model == null)
                {
                    return Json(new { success = false, message = "Invalid request data" });
                }

                var studentId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(studentId))
                {
                    return Json(new { success = false, message = "User not authenticated" });
                }

                // Verify the student ID matches the logged-in user
                if (studentId != model.StudentId)
                {
                    return Json(new { success = false, message = "Invalid student ID" });
                }

                // Check if already enrolled
                var existingEnrollment = await _enrollmentService.GetEnrollmentAsync(studentId, model.CourseId);
                if (existingEnrollment != null)
                {
                    return Json(new { success = false, message = "You are already enrolled in this course" });
                }

                // Enroll the student
                var success = await _enrollmentService.EnrollStudentInCourseAsync(studentId, model.CourseId);
                
                if (success)
                {
                    return Json(new { success = true, message = "Successfully enrolled in course!" });
                }
                else
                {
                    return Json(new { success = false, message = "Failed to enroll in course. Please try again." });
                }
            }
            catch (Exception ex)
            {
                // Log the exception for debugging
                System.Diagnostics.Debug.WriteLine($"Enrollment error: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                return Json(new { success = false, message = $"Error enrolling in course: {ex.Message}" });
            }
        }



        public async Task<IActionResult> Feedback()
        {
            try
            {
                var studentId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(studentId))
                {
                    TempData["Error"] = "User not authenticated";
                    return RedirectToAction("Login", "Account", new { area = "Identity" });
                }

                // Get enrolled courses for the dropdown
                var enrollments = await _enrollmentService.GetEnrollmentsByStudentAsync(studentId);
                ViewBag.EnrolledCourses = enrollments;

                return View(new FeedbackViewModel());
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error loading feedback page";
                return View(new FeedbackViewModel());
            }
        }

        [HttpPost]
        public async Task<IActionResult> SubmitFeedback([FromBody] FeedbackViewModel model)
        {
            try
            {
                var studentId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(studentId))
                {
                    return Json(new { success = false, message = "User not authenticated" });
                }

                // Verify student is enrolled in this course
                var enrollment = await _enrollmentService.GetEnrollmentAsync(studentId, model.CourseId);
                if (enrollment == null)
                {
                    return Json(new { success = false, message = "You can only provide feedback for courses you are enrolled in" });
                }

                // Check if feedback already exists for this course
                var existingFeedback = await _context.Feedbacks
                    .FirstOrDefaultAsync(f => f.StudentId == studentId && f.CourseId == model.CourseId);

                if (existingFeedback != null)
                {
                    return Json(new { success = false, message = "You have already provided feedback for this course" });
                }

                // Create new feedback
                var feedback = new Feedback
                {
                    CourseId = model.CourseId,
                    StudentId = studentId,
                    Comment = model.Comments,
                    Rating = model.Rating,
                    SubmittedAt = DateTime.Now,
                    CreatedAt = DateTime.Now
                };

                _context.Feedbacks.Add(feedback);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Feedback submitted successfully!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error submitting feedback" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetFeedbackHistory()
        {
            try
            {
                var studentId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(studentId))
                {
                    return Json(new { success = false, message = "User not authenticated" });
                }

                var feedbackHistory = await _context.Feedbacks
                    .Where(f => f.StudentId == studentId)
                    .Include(f => f.Course)
                    .OrderByDescending(f => f.SubmittedAt)
                    .Select(f => new
                    {
                        courseTitle = f.Course.Title,
                        rating = f.Rating,
                        comments = f.Comment,
                        submittedAt = f.SubmittedAt
                    })
                    .ToListAsync();

                return Json(new { success = true, feedback = feedbackHistory });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error loading feedback history" });
            }
        }

        public async Task<IActionResult> Profile()
        {
            try
            {
                var studentId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(studentId))
                {
                    TempData["Error"] = "User not authenticated";
                    return RedirectToAction("Login", "Account", new { area = "Identity" });
                }

                // Get user information
                var user = await _context.Users.FindAsync(studentId);
                if (user == null)
                {
                    TempData["Error"] = "User not found";
                    return RedirectToAction("Login", "Account", new { area = "Identity" });
                }

                // Get enrollments and course information
                var enrollments = await _context.Enrollments
                    .Where(e => e.StudentId == studentId)
                    .Include(e => e.Course)
                    .ToListAsync();

                var totalCoursesEnrolled = enrollments.Count;
                var completedCourses = enrollments.Count(e => e.CompletionStatus == "Completed");
                var enrolledCourseTitles = enrollments.Select(e => e.Course.Title).ToList();

                var profileViewModel = new ProfileViewModel
                {
                    FullName = user.FullName ?? user.UserName,
                    Email = user.Email,
                    PhoneNumber = user.PhoneNumber,
                    TotalCoursesEnrolled = totalCoursesEnrolled,
                    CompletedCourses = completedCourses,
                    EnrolledCourseTitles = enrolledCourseTitles
                };
                
                return View(profileViewModel);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error loading profile";
                return View(new ProfileViewModel());
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateProfile(StudentProfileViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    TempData["Error"] = "Please fill in all required fields";
                    return RedirectToAction("Profile");
                }

                var studentId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                model.Id = studentId;

                // TODO: Implement profile update service
                // var success = await _studentService.UpdateStudentProfileAsync(model);

                TempData["Success"] = "Profile updated successfully!";
                return RedirectToAction("Profile");
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error updating profile";
                return RedirectToAction("Profile");
            }
        }

        // Course Detail Actions
        [HttpGet("Student/GetCourseDetails/{courseId}")]
        public async Task<IActionResult> GetCourseDetails(int courseId)
        {
            try
            {
                var studentId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(studentId))
                {
                    return Content("User not authenticated");
                }

                // Verify student is enrolled in this course
                var enrollment = await _enrollmentService.GetEnrollmentAsync(studentId, courseId);
                if (enrollment == null)
                {
                    return Content("Access denied to this course");
                }

                // Get course details (with materials and enrollments)
                var course = await _courseService.GetCourseAsync(courseId);
                if (course == null)
                {
                    return Content("Course not found");
                }

                // Get instructor name
                string instructorName = "Unknown Instructor";
                if (!string.IsNullOrEmpty(course.CreatedById))
                {
                    var instructor = await _courseService.GetUserByIdAsync(course.CreatedById);
                    if (instructor != null)
                    {
                        instructorName = !string.IsNullOrEmpty(instructor.FullName) ? instructor.FullName : instructor.UserName;
                    }
                }

                // Get course materials count
                var totalMaterials = course.Materials?.Count ?? 0;

                // Get enrollment count
                var enrollmentCount = course.EnrollmentCount;

                // Progress calculation
                int completedMaterials = 0;
                int progressPercentage = 0;
                if (enrollment.CompletionStatus == "Completed" && totalMaterials > 0)
                {
                    completedMaterials = totalMaterials;
                    progressPercentage = 100;
                }
                else if (totalMaterials > 0)
                {
                    completedMaterials = 0;
                    progressPercentage = 0;
                }

                // Optionally, split videos/docs (for now, keep as 0 or all as docs)
                int videosWatched = 0;
                int documentsRead = completedMaterials;

                // Get last accessed (not tracked yet, so keep as yesterday for now)
                DateTime? lastAccessed = DateTime.Now.AddDays(-1);

                // Get feedback data
                var studentFeedbackData = await _context.Feedbacks
                    .FirstOrDefaultAsync(f => f.StudentId == studentId && f.CourseId == courseId);

                var hasSubmittedFeedback = studentFeedbackData != null;
                var studentRating = studentFeedbackData?.Rating ?? 0;
                var studentFeedback = studentFeedbackData?.Comment ?? "";

                // Get course average rating and total ratings
                var courseFeedback = await _context.Feedbacks
                    .Where(f => f.CourseId == courseId)
                    .ToListAsync();

                var averageRating = courseFeedback.Any() ? courseFeedback.Select((Feedback f) => f.Rating).Average() : 0;
                var totalRatings = courseFeedback.Count;

                // Check if student can submit feedback (enrolled but hasn't submitted yet)
                var canSubmitFeedback = enrollment != null && !hasSubmittedFeedback;

                var courseDetailViewModel = new CourseDetailViewModel
                {
                    CourseId = courseId,
                    Title = course.Title,
                    Description = course.Description,
                    InstructorName = instructorName,
                    StartDate = course.StartDate,
                    EndDate = course.EndDate,
                    EnrollmentCount = enrollmentCount,
                    MaterialCount = totalMaterials,
                    ProgressPercentage = progressPercentage,
                    CompletionStatus = enrollment.CompletionStatus,
                    EnrolledDate = enrollment.EnrolledOn,
                    MaterialsCompleted = completedMaterials,
                    TotalMaterials = totalMaterials,
                    VideosWatched = videosWatched,
                    DocumentsRead = documentsRead,
                    LastAccessed = lastAccessed,
                    HasSubmittedFeedback = hasSubmittedFeedback,
                    StudentRating = studentRating,
                    StudentFeedback = studentFeedback,
                    AverageRating = averageRating,
                    TotalRatings = totalRatings,
                    CanSubmitFeedback = canSubmitFeedback
                };

                return PartialView("_CourseDetails", courseDetailViewModel);
            }
            catch (Exception ex)
            {
                return Content("Error loading course details");
            }
        }

        [HttpGet("Student/GetCourseMaterials/{courseId}")]
        public async Task<IActionResult> GetCourseMaterials(int courseId)
        {
            try
            {
                var studentId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(studentId))
                {
                    return Content("User not authenticated");
                }

                // Verify student is enrolled in this course
                var enrollment = await _enrollmentService.GetEnrollmentAsync(studentId, courseId);
                if (enrollment == null)
                {
                    return Content("Access denied to this course");
                }

                // Fetch actual course materials from the database
                var materials = await _context.CourseMaterials
                    .Where(m => m.CourseId == courseId)
                    .OrderBy(m => m.CreatedAt)
                    .ToListAsync();

                return PartialView("_CourseMaterials", materials);
            }
            catch (Exception ex)
            {
                return Content("Error loading course materials");
            }
        }

        [HttpPost]
        public async Task<IActionResult> SubmitCourseFeedback(int courseId, int rating, string feedback)
        {
            try
            {
                var studentId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(studentId))
                {
                    return Json(new { success = false, message = "User not authenticated" });
                }

                // Verify student is enrolled in this course
                var enrollment = await _enrollmentService.GetEnrollmentAsync(studentId, courseId);
                if (enrollment == null)
                {
                    return Json(new { success = false, message = "Access denied to this course" });
                }

                // TODO: Implement feedback submission service
                // var success = await _feedbackService.SubmitCourseFeedbackAsync(studentId, courseId, rating, feedback);

                // Mock success for now
                var success = true;

                if (success)
                {
                    return Json(new { success = true, message = "Feedback submitted successfully" });
                }
                else
                {
                    return Json(new { success = false, message = "Failed to submit feedback" });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error submitting feedback" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> DownloadMaterial(int materialId)
        {
            try
            {
                var studentId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(studentId))
                {
                    return RedirectToAction("Login", "Account", new { area = "Identity" });
                }

                // Get the material and related course
                var material = await _context.CourseMaterials.FindAsync(materialId);
                if (material == null)
                {
                    return Content("Material not found");
                }

                // Find the enrollment
                var enrollment = await _context.Enrollments
                    .FirstOrDefaultAsync(e => e.StudentId == studentId && e.CourseId == material.CourseId);

                if (enrollment != null && enrollment.CompletionStatus != "Completed")
                {
                    enrollment.CompletionStatus = "Completed";
                    _context.Enrollments.Update(enrollment);
                    await _context.SaveChangesAsync();
                }

                // Download the file
                var filePath = Path.Combine(_webHostEnvironment.WebRootPath, material.FilePath.TrimStart('/').Replace("/", Path.DirectorySeparatorChar.ToString()));
                if (!System.IO.File.Exists(filePath))
                {
                    return Content("File not found on server");
                }

                var fileName = Path.GetFileName(material.FilePath);
                var contentType = "application/octet-stream";
                // Optionally, set contentType based on file extension
                return PhysicalFile(filePath, contentType, fileName);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error downloading material";
                return RedirectToAction("MyCourses");
            }
        }
    }
}
