using LMSApp.Models.ViewModels;

namespace LMSApp.Services
{
    public interface IEnrollmentService
    {
        Task<IEnumerable<EnrollmentViewModel>> GetEnrollmentsByCourseAsync(int courseId, string instructorId);
        Task<bool> VerifyCourseOwnershipAsync(int courseId, string instructorId);
        Task<IEnumerable<EnrollmentViewModel>> GetEnrollmentsByStudentAsync(string studentId);
        Task<bool> EnrollStudentInCourseAsync(string studentId, int courseId);
        Task<EnrollmentViewModel?> GetEnrollmentAsync(string studentId, int courseId);
    }
} 