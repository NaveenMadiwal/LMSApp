using LMSApp.Data;
using LMSApp.Models;
using LMSApp.Models.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace LMSApp.Services
{
    public class CourseService : ICourseService
    {
        private readonly ApplicationDbContext _context;

        public CourseService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<CourseListViewModel>> GetCoursesByCreatorAsync(string creatorId)
        {
            return await _context.Courses
                .Where(c => c.CreatedById == creatorId && c.IsActive)
                .Include(c => c.Category)
                .Include(c => c.CreatedBy)
                .Include(c => c.CourseMaterials)
                .Include(c => c.Enrollments)
                .Select(c => new CourseListViewModel
                {
                    Id = c.Id,
                    Title = c.Title,
                    Description = c.Description,
                    StartDate = c.StartDate,
                    EndDate = c.EndDate,
                    CategoryId = c.CategoryId,
                    CategoryName = c.Category != null ? c.Category.Name : "Uncategorized",
                    CreatedById = c.CreatedById,
                    CreatedByName = c.CreatedBy != null ? c.CreatedBy.UserName : "Unknown",
                    MaterialCount = c.CourseMaterials != null ? c.CourseMaterials.Count : 0,
                    EnrollmentCount = c.Enrollments != null ? c.Enrollments.Count : 0,
                    IsActive = c.IsActive,
                    CreatedAt = c.CreatedAt
                })
                .ToListAsync();
        }

        public async Task<CourseViewModel> GetCourseAsync(int id)
        {
            var course = await _context.Courses
                .Include(c => c.Category)
                .Include(c => c.CreatedBy)
                .Include(c => c.CourseMaterials)
                .Include(c => c.Enrollments)
                .FirstOrDefaultAsync(c => c.Id == id && c.IsActive);

            if (course == null)
                return null;

            return new CourseViewModel
            {
                Id = course.Id,
                Title = course.Title,
                Description = course.Description,
                StartDate = course.StartDate,
                EndDate = course.EndDate,
                CategoryId = course.CategoryId,
                CategoryName = course.Category?.Name,
                CreatedById = course.CreatedById,
                CreatedByName = course.CreatedBy?.UserName,
                IsActive = course.IsActive,
                EnrollmentCount = course.Enrollments?.Count ?? 0,
                Materials = course.CourseMaterials?.Select(m => new CourseMaterialViewModel
                {
                    Id = m.Id,
                    Title = m.Title,
                    FileType = m.FileType,
                    FilePath = m.FilePath,
                    CourseId = m.CourseId,
                    CreatedAt = m.CreatedAt
                }).ToList() ?? new List<CourseMaterialViewModel>()
            };
        }

        public async Task<bool> CreateCourseAsync(CourseViewModel model)
        {
            try
            {
                var course = new Course
                {
                    Title = model.Title,
                    Description = model.Description,
                    StartDate = model.StartDate,
                    EndDate = model.EndDate,
                    CategoryId = model.CategoryId,
                    CreatedById = model.CreatedById,
                    IsActive = model.IsActive,
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

        public async Task<bool> UpdateCourseAsync(int id, CourseViewModel model)
        {
            try
            {
                var course = await _context.Courses.FindAsync(id);
                if (course == null)
                    return false;

                // Ensure the course belongs to the current user
                if (course.CreatedById != model.CreatedById)
                    return false;

                course.Title = model.Title;
                course.Description = model.Description;
                course.StartDate = model.StartDate;
                course.EndDate = model.EndDate;
                course.CategoryId = model.CategoryId;
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

        public async Task<bool> DeleteCourseAsync(int id, string userId = null)
        {
            try
            {
                var course = await _context.Courses.FindAsync(id);
                if (course == null)
                    return false;

                // If userId is provided, ensure the course belongs to the user
                if (!string.IsNullOrEmpty(userId) && course.CreatedById != userId)
                    return false;

                // Soft delete - set IsActive to false
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

        public async Task<IEnumerable<CourseListViewModel>> GetAvailableCoursesForStudentAsync(string studentId)
        {
            // Get all active courses that the student is not enrolled in
            var enrolledCourseIds = await _context.Enrollments
                .Where(e => e.StudentId == studentId && e.IsActive)
                .Select(e => e.CourseId)
                .ToListAsync();

            return await _context.Courses
                .Where(c => c.IsActive && !enrolledCourseIds.Contains(c.Id))
                .Include(c => c.Category)
                .Include(c => c.CreatedBy)
                .Include(c => c.CourseMaterials)
                .Include(c => c.Enrollments)
                .Select(c => new CourseListViewModel
                {
                    Id = c.Id,
                    Title = c.Title,
                    Description = c.Description,
                    StartDate = c.StartDate,
                    EndDate = c.EndDate,
                    CategoryId = c.CategoryId,
                    CategoryName = c.Category != null ? c.Category.Name : "Uncategorized",
                    CreatedById = c.CreatedById,
                    CreatedByName = c.CreatedBy != null ? c.CreatedBy.UserName : "Unknown",
                    MaterialCount = c.CourseMaterials != null ? c.CourseMaterials.Count : 0,
                    EnrollmentCount = c.Enrollments != null ? c.Enrollments.Count : 0,
                    IsActive = c.IsActive,
                    CreatedAt = c.CreatedAt
                })
                .ToListAsync();
        }

        public async Task<ApplicationUser?> GetUserByIdAsync(string userId)
        {
            return await _context.Users.FindAsync(userId);
        }

        public async Task<bool> UpdateUserProfileAsync(string userId, ProfileViewModel model)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return false;
            user.FullName = model.FullName;
            user.Email = model.Email;
            user.PhoneNumber = model.PhoneNumber;
            user.DateOfBirth = model.DateOfBirth;
            // Do not update RoleType, IsActive, or LastLogin from profile form
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
            return true;
        }
    }
} 