using LMSApp.Data;
using LMSApp.Models;
using LMSApp.Models.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace LMSApp.Services
{
    public class EnrollmentService : IEnrollmentService
    {
        private readonly ApplicationDbContext _context;

        public EnrollmentService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<EnrollmentViewModel>> GetEnrollmentsByCourseAsync(int courseId, string instructorId)
        {
            // First verify that the course belongs to the instructor
            if (!await VerifyCourseOwnershipAsync(courseId, instructorId))
            {
                return new List<EnrollmentViewModel>();
            }

            return await _context.Enrollments
                .Where(e => e.CourseId == courseId && e.IsActive)
                .Include(e => e.Student)
                .Include(e => e.Course)
                .Select(e => new EnrollmentViewModel
                {
                    Id = e.Id,
                    StudentId = e.StudentId,
                    StudentName = e.Student != null ? e.Student.UserName : "Unknown",
                    StudentEmail = e.Student != null ? e.Student.Email : "",
                    CourseId = e.CourseId,
                    CourseTitle = e.Course != null ? e.Course.Title : "Unknown",
                    EnrolledOn = e.EnrolledOn,
                    CompletionStatus = e.CompletionStatus,
                    IsActive = e.IsActive
                })
                .ToListAsync();
        }

        public async Task<bool> VerifyCourseOwnershipAsync(int courseId, string instructorId)
        {
            var course = await _context.Courses
                .Where(c => c.Id == courseId && c.IsActive)
                .FirstOrDefaultAsync();

            return course != null && course.CreatedById == instructorId;
        }

        public async Task<IEnumerable<EnrollmentViewModel>> GetEnrollmentsByStudentAsync(string studentId)
        {
            return await _context.Enrollments
                .Where(e => e.StudentId == studentId && e.IsActive)
                .Include(e => e.Student)
                .Include(e => e.Course)
                .Select(e => new EnrollmentViewModel
                {
                    Id = e.Id,
                    StudentId = e.StudentId,
                    StudentName = e.Student != null ? e.Student.UserName : "Unknown",
                    StudentEmail = e.Student != null ? e.Student.Email : "",
                    CourseId = e.CourseId,
                    CourseTitle = e.Course != null ? e.Course.Title : "Unknown",
                    EnrolledOn = e.EnrolledOn,
                    CompletionStatus = e.CompletionStatus,
                    IsActive = e.IsActive
                })
                .ToListAsync();
        }

        public async Task<bool> EnrollStudentInCourseAsync(string studentId, int courseId)
        {
            try
            {
                // Check if student is already enrolled
                var existingEnrollment = await _context.Enrollments
                    .FirstOrDefaultAsync(e => e.StudentId == studentId && e.CourseId == courseId && e.IsActive);

                if (existingEnrollment != null)
                {
                    return false; // Already enrolled
                }

                // Check if course exists and is active
                var course = await _context.Courses
                    .FirstOrDefaultAsync(c => c.Id == courseId && c.IsActive);

                if (course == null)
                {
                    return false; // Course not found or inactive
                }

                var enrollment = new Enrollment
                {
                    StudentId = studentId,
                    CourseId = courseId,
                    EnrolledOn = DateTime.Now,
                    CompletionStatus = "Pending",
                    IsActive = true,
                    CreatedAt = DateTime.Now
                };

                _context.Enrollments.Add(enrollment);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<EnrollmentViewModel?> GetEnrollmentAsync(string studentId, int courseId)
        {
            return await _context.Enrollments
                .Where(e => e.StudentId == studentId && e.CourseId == courseId && e.IsActive)
                .Include(e => e.Student)
                .Include(e => e.Course)
                .Select(e => new EnrollmentViewModel
                {
                    Id = e.Id,
                    StudentId = e.StudentId,
                    StudentName = e.Student != null ? e.Student.UserName : "Unknown",
                    CourseId = e.CourseId,
                    CourseTitle = e.Course != null ? e.Course.Title : "Unknown",
                    EnrolledOn = e.EnrolledOn,
                    CompletionStatus = e.CompletionStatus,
                    IsActive = e.IsActive
                })
                .FirstOrDefaultAsync();
        }
    }
} 