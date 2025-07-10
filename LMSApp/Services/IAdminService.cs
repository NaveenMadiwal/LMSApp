using LMSApp.Models.ViewModels;
using LMSApp.Models;

namespace LMSApp.Services
{
    public interface IAdminService
    {
        // Dashboard Statistics
        Task<AdminDashboardViewModel> GetDashboardStatisticsAsync();
        
        // Category Management
        Task<List<Category>> GetCategoriesAsync();
        Task<bool> CreateCategoryAsync(CategoryViewModel model);
        Task<bool> UpdateCategoryAsync(CategoryViewModel model);
        Task<bool> DeleteCategoryAsync(int id);
        
        // Course Management
        Task<List<CourseListViewModel>> GetCoursesAsync();
        Task<bool> CreateCourseAsync(CourseViewModel model);
        Task<bool> UpdateCourseAsync(CourseViewModel model);
        Task<bool> DeleteCourseAsync(int id);
        
        // User Management
        Task<List<StudentListViewModel>> GetStudentsAsync();
        Task<List<StudentListViewModel>> GetStudentsWithEnrollmentsAsync();
        Task<bool> CreateStudentAsync(StudentViewModel model);
        Task<bool> UpdateStudentAsync(StudentViewModel model);
        Task<bool> DeleteStudentAsync(string id);
        
        // Feedback Management
        Task<List<FeedbackViewModel>> GetFeedbackAsync();
        Task<FeedbackAnalyticsViewModel> GetFeedbackAnalyticsAsync();
        Task<FeedbackViewModel?> GetFeedbackByIdAsync(int feedbackId);
        
        // Reports
        Task<AdminReportsViewModel> GetReportsAsync();
        
        // Activity Logs
        Task<List<ActivityLogViewModel>> GetRecentActivityAsync();
        Task<bool> UpdateUserActiveOrInactive(StudentViewModel model);
    }
} 