using System.ComponentModel.DataAnnotations;

namespace LMSApp.Models.ViewModels
{
    public class ProfileViewModel
    {
        public string FullName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string RoleType { get; set; }
        public bool IsActive { get; set; }
        public DateTime? LastLogin { get; set; }

        public int TotalCoursesEnrolled { get; set; }
        public int CompletedCourses { get; set; }
        public List<string> EnrolledCourseTitles { get; set; } = new List<string>();
    }
} 