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

}
