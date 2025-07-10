using LMSApp.Models.ViewModels;
using LMSApp.Models;
using System.Threading.Tasks;

namespace LMSApp.Services
{
    public interface ICourseService
    {
        Task<IEnumerable<CourseListViewModel>> GetCoursesByCreatorAsync(string creatorId);
        Task<CourseViewModel> GetCourseAsync(int id);
        Task<bool> CreateCourseAsync(CourseViewModel model);
        Task<bool> UpdateCourseAsync(int id, CourseViewModel model);
        Task<bool> DeleteCourseAsync(int id, string userId = null);
        Task<IEnumerable<CourseListViewModel>> GetAvailableCoursesForStudentAsync(string studentId);
        Task<ApplicationUser?> GetUserByIdAsync(string userId);
        Task<bool> UpdateUserProfileAsync(string userId, ProfileViewModel model);
    }
} 