using LMSApp.Data;
using LMSApp.Models;
using LMSApp.Models.ViewModels;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace LMSApp.Services
{
    public class AdminService : IAdminService
    {
        private readonly ApplicationDbContext _context;

        public AdminService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<AdminDashboardViewModel> GetDashboardStatisticsAsync()
        {
            var totalCourses = await _context.Courses.CountAsync(c => c.IsActive);
            var totalEnrollments = await _context.Enrollments.CountAsync(e => e.IsActive);
            var totalStudents = await _context.Users.CountAsync(u => u.RoleType == "Student");
            var totalCategories = await _context.Categories.CountAsync(c => c.IsActive);

            return new AdminDashboardViewModel
            {
                TotalCourses = totalCourses,
                TotalEnrollments = totalEnrollments,
                TotalStudents = totalStudents,
                TotalCategories = totalCategories
            };
        }

        public async Task<List<Category>> GetCategoriesAsync()
        {
            return await _context.Categories
                .Where(c => c.IsActive)
                .OrderBy(c => c.Name)
                .ToListAsync();
        }

        public async Task<bool> CreateCategoryAsync(CategoryViewModel model)
        {
            try
            {
                var category = new Category
                {
                    Name = model.Name,
                    IsActive = true,
                    CreatedAt = DateTime.Now
                };

                _context.Categories.Add(category);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> UpdateCategoryAsync(CategoryViewModel model)
        {
            try
            {
                var category = await _context.Categories.FindAsync(model.Id);
                if (category == null) return false;

                category.Name = model.Name;
                category.UpdatedAt = DateTime.Now;

                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> DeleteCategoryAsync(int id)
        {
            try
            {
                var category = await _context.Categories.FindAsync(id);
                if (category == null) return false;

                category.IsActive = false;
                category.UpdatedAt = DateTime.Now;

                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<List<CourseListViewModel>> GetCoursesAsync()
        {
            return await _context.Courses
                .Where(c => c.IsActive)
                .Include(c => c.Category)
                .Include(c => c.CreatedBy)
                .Select(c => new CourseListViewModel
                {
                    Id = c.Id,
                    Title = c.Title,
                    Description = c.Description,
                    CategoryId = c.CategoryId,
                    CategoryName = c.Category != null ? c.Category.Name : "Uncategorized",
                    CreatedById = c.CreatedById,
                    CreatedByName = c.CreatedBy != null ? (c.CreatedBy.FullName ?? c.CreatedBy.UserName) : "Unknown",
                    StartDate = c.StartDate,
                    EndDate = c.EndDate,
                    IsActive = c.IsActive,
                    EnrollmentCount = c.Enrollments.Count(e => e.IsActive),
                    MaterialCount = c.CourseMaterials.Count,
                    CreatedAt = c.CreatedAt
                })
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();
        }

        public async Task<bool> CreateCourseAsync(CourseViewModel model)
        {
            try
            {
                var course = new Course
                {
                    Title = model.Title,
                    Description = model.Description,
                    CategoryId = model.CategoryId,
                    StartDate = model.StartDate,
                    EndDate = model.EndDate,
                    IsActive = model.IsActive,
                    CreatedById = model.CreatedById,
                    CreatedAt = DateTime.Now
                };

                _context.Courses.Add(course);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> UpdateCourseAsync(CourseViewModel model)
        {
            try
            {
                var course = await _context.Courses.FindAsync(model.Id);
                if (course == null) return false;

                course.Title = model.Title;
                course.Description = model.Description;
                course.CategoryId = model.CategoryId;
                course.StartDate = model.StartDate;
                course.EndDate = model.EndDate;
                course.IsActive = model.IsActive;
                course.UpdatedAt = DateTime.Now;

                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> DeleteCourseAsync(int id)
        {
            try
            {
                var course = await _context.Courses.FindAsync(id);
                if (course == null) return false;

                course.IsActive = false;
                course.UpdatedAt = DateTime.Now;

                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<List<StudentListViewModel>> GetStudentsAsync()
        {
            return await _context.Users
                .Select(u => new StudentListViewModel
                {
                    Id = u.Id,
                    UserName = u.UserName,
                    Email = u.Email,
                    FullName = u.FullName,
                    PhoneNumber = u.PhoneNumber,
                    RoleType = u.RoleType,
                    IsActive = u.IsActive,
                    EnrollmentCount = u.Enrollments.Count(e => e.IsActive),
                    CompletedCourses = u.Enrollments.Count(e => e.CompletionStatus == "Completed"),
                    CreatedAt = DateTime.Now, // or u.CreatedAt if available
                    LastLoginDate = null, // or u.LastLoginDate if available
                    DateOfBirth = u.DateOfBirth// Set to null if not available on ApplicationUser
                })
                .OrderBy(u => u.FullName)
                .ToListAsync();
        }

        public async Task<List<StudentListViewModel>> GetStudentsWithEnrollmentsAsync()
        {
            // Get all students first
            var students = await _context.Users
                .Where(u => u.RoleType == "Student")
                .Select(u => new StudentListViewModel
                {
                    Id = u.Id,
                    UserName = u.UserName,
                    Email = u.Email,
                    FullName = u.FullName,
                    PhoneNumber = u.PhoneNumber,
                    RoleType = u.RoleType,
                    IsActive = u.IsActive,
                    EnrollmentCount = 0, // Will be calculated separately
                    CompletedCourses = 0, // Will be calculated separately
                    CreatedAt = DateTime.Now,
                    LastLoginDate = null,
                    DateOfBirth = null
                })
                .OrderBy(u => u.FullName)
                .ToListAsync();

            // Debug: Check what users are being returned
            Console.WriteLine($"Found {students.Count} students:");
            foreach (var student in students)
            {
                Console.WriteLine($"- {student.FullName} ({student.UserName}) - Role: {student.RoleType}");
            }

            // Get enrollment counts separately
            var enrollmentCounts = await _context.Enrollments
                .GroupBy(e => e.StudentId)
                .Select(g => new { StudentId = g.Key, Count = g.Count(e => e.IsActive), Completed = g.Count(e => e.CompletionStatus == "Completed") })
                .ToListAsync();

            // Update the counts
            foreach (var student in students)
            {
                var enrollment = enrollmentCounts.FirstOrDefault(e => e.StudentId == student.Id);
                if (enrollment != null)
                {
                    student.EnrollmentCount = enrollment.Count;
                    student.CompletedCourses = enrollment.Completed;
                }
            }

            return students;
        }

        public async Task<bool> CreateStudentAsync(StudentViewModel model)
        {
            try
            {
                var user = new ApplicationUser
                {
                    UserName = model.Email,
                    Email = model.Email,
                    FullName = model.FullName,
                    PhoneNumber = model.PhoneNumber,
                    RoleType = "Student",
                    IsActive = true,
                    EmailConfirmed = true
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> UpdateStudentAsync(StudentViewModel model)
        {
            try
            {
                var user = await _context.Users.FindAsync(model.Id);
                if (user == null) return false;

                user.FullName = model.FullName;
                user.Email = model.Email;
                user.UserName = model.UserName;
                user.PhoneNumber = model.PhoneNumber;
                user.IsActive = model.IsActive;
                user.RoleType = model.RoleType;
                user.DateOfBirth = model.DateOfBirth;

                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> UpdateUserActiveOrInactive(StudentViewModel model)
        {
            try
            {
                var user = await _context.Users.FindAsync(model.Id);
                if (user == null) return false;
                 
                user.IsActive = model.IsActive; 

                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

            public async Task<bool> DeleteStudentAsync(string id)
        {
            try
            {
                var user = await _context.Users.FindAsync(id);
                if (user == null) return false;

                user.IsActive = false;

                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<List<FeedbackViewModel>> GetFeedbackAsync()
        {
            return await _context.Feedbacks
                .Include(f => f.Student)
                .Include(f => f.Course)
                .OrderByDescending(f => f.SubmittedAt)
                .Select(f => new FeedbackViewModel
                {
                    Id = f.Id,
                    CourseId = f.CourseId,
                    CourseTitle = f.Course.Title,
                    StudentId = f.StudentId,
                    StudentName = f.Student.FullName ?? f.Student.UserName,
                    Rating = f.Rating,
                    Comments = f.Comment,
                    SubmittedAt = f.SubmittedAt
                })
                .ToListAsync();
        }

        public async Task<FeedbackViewModel?> GetFeedbackByIdAsync(int feedbackId)
        {
            var feedback = await _context.Feedbacks
                .Include(f => f.Student)
                .Include(f => f.Course)
                .FirstOrDefaultAsync(f => f.Id == feedbackId);
            if (feedback == null) return null;
            return new FeedbackViewModel
            {
                Id = feedback.Id,
                StudentId = feedback.StudentId,
                StudentName = feedback.Student?.FullName ?? "N/A",
                CourseId = feedback.CourseId,
                CourseTitle = feedback.Course?.Title ?? "N/A",
                Rating = feedback.Rating,
                Comments = feedback.Comment,
                SubmittedAt = feedback.CreatedAt
            };
        }

        public async Task<FeedbackAnalyticsViewModel> GetFeedbackAnalyticsAsync()
        {
            var feedback = await _context.Feedbacks
                .Include(f => f.Course)
                .Include(f => f.Course.CreatedBy)
                .ToListAsync();

            var totalFeedback = feedback.Count;
            var averageRating = feedback.Any() ? feedback.Average(f => f.Rating) : 0;
            var positiveFeedback = feedback.Count(f => f.Rating >= 4);
            var negativeFeedback = feedback.Count(f => f.Rating <= 2);
            var neutralFeedback = feedback.Count(f => f.Rating == 3);

            // Get top and low rated courses
            var courseRatings = feedback
                .GroupBy(f => new { f.CourseId, f.Course.Title, InstructorName = f.Course.CreatedBy.FullName ?? f.Course.CreatedBy.UserName })
                .Select(g => new CourseFeedbackViewModel
                {
                    CourseId = g.Key.CourseId,
                    CourseTitle = g.Key.Title,
                    InstructorName = g.Key.InstructorName,
                    AverageRating = g.Average(f => f.Rating),
                    FeedbackCount = g.Count()
                })
                .OrderByDescending(c => c.AverageRating)
                .ToList();

            return new FeedbackAnalyticsViewModel
            {
                TotalFeedback = totalFeedback,
                AverageRating = Math.Round(averageRating, 1),
                PositiveFeedback = positiveFeedback,
                NegativeFeedback = negativeFeedback,
                NeutralFeedback = neutralFeedback,
                TopRatedCourses = courseRatings.Take(5).ToList(),
                LowRatedCourses = courseRatings.TakeLast(5).ToList()
            };
        }

        public async Task<AdminReportsViewModel> GetReportsAsync()
        {
            var enrollments = await _context.Enrollments.Include(e => e.Course).ToListAsync();
            var courses = await _context.Courses.Include(c => c.CreatedBy).ToListAsync();
            var users = await _context.Users.Where(u => u.RoleType == "Student").ToListAsync();
            var feedbacks = await _context.Feedbacks.Include(f => f.Course).Include(f => f.Course.CreatedBy).ToListAsync();

            // Top Rated Courses
            var highestRatedCourses = feedbacks
                .GroupBy(f => new { f.CourseId, f.Course.Title, InstructorName = f.Course.CreatedBy.FullName ?? f.Course.CreatedBy.UserName })
                .Select(g => new TopCourseViewModel
                {
                    CourseId = g.Key.CourseId,
                    CourseTitle = g.Key.Title,
                    InstructorName = g.Key.InstructorName,
                    AverageRating = g.Average(f => f.Rating),
                    EnrollmentCount = enrollments.Count(e => e.CourseId == g.Key.CourseId)
                })
                .OrderByDescending(c => c.AverageRating)
                .Take(5)
                .ToList();

            // Most Popular Courses
            var mostPopularCourses = enrollments
                .GroupBy(e => new { e.CourseId, e.Course.Title, InstructorName = e.Course.CreatedBy.FullName ?? e.Course.CreatedBy.UserName })
                .Select(g => new TopCourseViewModel
                {
                    CourseId = g.Key.CourseId,
                    CourseTitle = g.Key.Title,
                    InstructorName = g.Key.InstructorName,
                    EnrollmentCount = g.Count(),
                    AverageRating = feedbacks.Where(f => f.CourseId == g.Key.CourseId).DefaultIfEmpty().Average(f => f == null ? 0 : f.Rating)
                })
                .OrderByDescending(c => c.EnrollmentCount)
                .Take(5)
                .ToList();

            // Most Active Students
            var mostActiveUsers = enrollments
                .GroupBy(e => e.StudentId)
                .Select(g => {
                    var user = users.FirstOrDefault(u => u.Id == g.Key);
                    return new UserActivityViewModel
                    {
                        UserId = g.Key,
                        UserName = user?.FullName ?? user?.UserName ?? "N/A",
                        Email = user?.Email ?? "",
                        EnrolledCourses = g.Count(),
                        CompletedCourses = g.Count(e => e.CompletionStatus == "Completed"),
                      //  LastLogin = user?.LastLogin ?? DateTime.MinValue
                    };
                })
                .OrderByDescending(u => u.EnrolledCourses)
                .ThenByDescending(u => u.CompletedCourses)
                .Take(10)
                .ToList();

            return new AdminReportsViewModel
            {
                EnrollmentReport = new EnrollmentReportViewModel
                {
                    TotalEnrollments = enrollments.Count,
                    ActiveEnrollments = enrollments.Count(e => e.IsActive),
                    CompletedEnrollments = enrollments.Count(e => e.CompletionStatus == "Completed"),
                    CompletionRate = enrollments.Any() ? (double)enrollments.Count(e => e.CompletionStatus == "Completed") / enrollments.Count * 100 : 0
                },
                CourseReport = new CourseReportViewModel
                {
                    TotalCourses = courses.Count,
                    ActiveCourses = courses.Count(c => c.IsActive),
                    InactiveCourses = courses.Count(c => !c.IsActive),
                    AverageCourseRating = feedbacks.Any() ? feedbacks.Average(f => f.Rating) : 0,
                    HighestRatedCourses = highestRatedCourses,
                    MostPopularCourses = mostPopularCourses
                },
                UserReport = new UserReportViewModel
                {
                    TotalUsers = users.Count,
                    ActiveUsers = users.Count(u => u.IsActive),
                    NewUsersThisMonth = 0, // Simplified - would need to track user creation dates separately
                    MostActiveUsers = mostActiveUsers
                },
                RevenueReport = new RevenueReportViewModel
                {
                    TotalRevenue = 0, // Would need to implement payment system
                    CurrentMonthRevenue = 0,
                    AverageRevenuePerCourse = 0,
                    MonthlyRevenue = new List<MonthlyRevenueViewModel>()
                }
            };
        }

        public async Task<List<ActivityLogViewModel>> GetRecentActivityAsync()
        {
            // This is a placeholder implementation
            // In a real application, you'd have an ActivityLog table
            return new List<ActivityLogViewModel>
            {
                new ActivityLogViewModel
                {
                    Id = 1,
                    UserName = "System",
                    Action = "Dashboard Access",
                    Description = "Admin dashboard accessed",
                    Timestamp = DateTime.Now.AddMinutes(-5),
                    Severity = ActivitySeverity.Low
                }
            };
        }
    }
} 