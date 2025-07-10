using System.Security.Cryptography.X509Certificates;

namespace LMSApp.Models.ViewModels
{
    public class AdminDashboardViewModel
    {
        public int TotalCourses { get; set; }
        public int TotalEnrollments { get; set; } 
        public int TotalStudents { get; set; }
        public int TotalCategories { get; set; }
    }

    public class InstructorDashboardViewModel
    {
        public int CourseCount { get; set; }
        public int EnrolledStudentCount { get; set; }
        public List<CourseListViewModel> RecentCourses { get; set; } = new List<CourseListViewModel>();
        public List<EnrollmentViewModel> RecentEnrollments { get; set; } = new List<EnrollmentViewModel>();
    }
}
