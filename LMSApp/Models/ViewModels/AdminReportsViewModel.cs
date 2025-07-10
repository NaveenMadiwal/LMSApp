namespace LMSApp.Models.ViewModels
{
    public class AdminReportsViewModel
    {
        public EnrollmentReportViewModel EnrollmentReport { get; set; } = new EnrollmentReportViewModel();
        public CourseReportViewModel CourseReport { get; set; } = new CourseReportViewModel();
        public UserReportViewModel UserReport { get; set; } = new UserReportViewModel();
        public RevenueReportViewModel RevenueReport { get; set; } = new RevenueReportViewModel();
    }

    public class EnrollmentReportViewModel
    {
        public int TotalEnrollments { get; set; }
        public int ActiveEnrollments { get; set; }
        public int CompletedEnrollments { get; set; }
        public double CompletionRate { get; set; }
        public List<MonthlyEnrollmentViewModel> MonthlyEnrollments { get; set; } = new List<MonthlyEnrollmentViewModel>();
        public List<TopCourseViewModel> TopEnrolledCourses { get; set; } = new List<TopCourseViewModel>();
    }

    public class CourseReportViewModel
    {
        public int TotalCourses { get; set; }
        public int ActiveCourses { get; set; }
        public int InactiveCourses { get; set; }
        public double AverageCourseRating { get; set; }
        public List<TopCourseViewModel> MostPopularCourses { get; set; } = new List<TopCourseViewModel>();
        public List<TopCourseViewModel> HighestRatedCourses { get; set; } = new List<TopCourseViewModel>();
    }

    public class UserReportViewModel
    {
        public int TotalUsers { get; set; }
        public int ActiveUsers { get; set; }
        public int NewUsersThisMonth { get; set; }
        public List<MonthlyUserViewModel> MonthlyUsers { get; set; } = new List<MonthlyUserViewModel>();
        public List<UserActivityViewModel> MostActiveUsers { get; set; } = new List<UserActivityViewModel>();
    }

    public class RevenueReportViewModel
    {
        public decimal TotalRevenue { get; set; }
        public decimal CurrentMonthRevenue { get; set; }
        public decimal AverageRevenuePerCourse { get; set; }
        public List<MonthlyRevenueViewModel> MonthlyRevenue { get; set; } = new List<MonthlyRevenueViewModel>();
    }

    public class MonthlyEnrollmentViewModel
    {
        public string Month { get; set; }
        public int Enrollments { get; set; }
        public int Completions { get; set; }
    }

    public class TopCourseViewModel
    {
        public int CourseId { get; set; }
        public string CourseTitle { get; set; }
        public string InstructorName { get; set; }
        public int EnrollmentCount { get; set; }
        public double AverageRating { get; set; }
        public decimal Revenue { get; set; }
    }

    public class MonthlyUserViewModel
    {
        public string Month { get; set; }
        public int NewUsers { get; set; }
        public int ActiveUsers { get; set; }
    }

    public class UserActivityViewModel
    {
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public int EnrolledCourses { get; set; }
        public int CompletedCourses { get; set; }
        public DateTime LastLogin { get; set; }
    }

    public class MonthlyRevenueViewModel
    {
        public string Month { get; set; }
        public decimal Revenue { get; set; }
        public int CourseSales { get; set; }
    }
} 