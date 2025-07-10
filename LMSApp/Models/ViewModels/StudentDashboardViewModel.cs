using System.ComponentModel.DataAnnotations;

namespace LMSApp.Models.ViewModels
{
    public class StudentDashboardViewModel
    {
        public int TotalEnrollments { get; set; }
        public int ActiveCourses { get; set; }
        public int CompletedCourses { get; set; }
        public double AverageProgress { get; set; }
        public List<EnrollmentViewModel> RecentEnrollments { get; set; } = new List<EnrollmentViewModel>();
    }

    public class StudentProgressViewModel
    {
        public int TotalCourses { get; set; }
        public int CompletedCourses { get; set; }
        public int InProgressCourses { get; set; }
        public int PendingCourses { get; set; }
        public double AverageCompletionRate { get; set; }
        public List<EnrollmentViewModel> Enrollments { get; set; } = new List<EnrollmentViewModel>();
    }

    public class StudentProfileViewModel
    {
        public string Id { get; set; } = string.Empty;

        [Required(ErrorMessage = "Full name is required")]
        [StringLength(100, ErrorMessage = "Full name cannot exceed 100 characters")]
        [Display(Name = "Full Name")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Display(Name = "Phone Number")]
        public string? PhoneNumber { get; set; }

        [Display(Name = "Date of Birth")]
        [DataType(DataType.Date)]
        public DateTime? DateOfBirth { get; set; }

        [Display(Name = "Current Password")]
        [DataType(DataType.Password)]
        public string? CurrentPassword { get; set; }

        [Display(Name = "New Password")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters")]
        [DataType(DataType.Password)]
        public string? NewPassword { get; set; }

        [Display(Name = "Confirm New Password")]
        [Compare("NewPassword", ErrorMessage = "Passwords do not match")]
        [DataType(DataType.Password)]
        public string? ConfirmPassword { get; set; }
    }

    public class FeedbackViewModel
    {
        public int Id { get; set; }
        public int CourseId { get; set; }
        public string CourseTitle { get; set; } = string.Empty;
        public string StudentId { get; set; } = string.Empty;
        public string StudentName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please provide your feedback")]
        [StringLength(500, ErrorMessage = "Feedback cannot exceed 500 characters")]
        [Display(Name = "Comments")]
        public string Comments { get; set; } = string.Empty;

        [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5")]
        [Display(Name = "Rating")]
        public int Rating { get; set; }

        public DateTime SubmittedAt { get; set; }
    }
} 