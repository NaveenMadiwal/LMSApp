namespace LMSApp.Models.ViewModels
{
    public class FeedbackAnalyticsViewModel
    {
        public int TotalFeedback { get; set; }
        public double AverageRating { get; set; }
        public int PositiveFeedback { get; set; }
        public int NegativeFeedback { get; set; }
        public int NeutralFeedback { get; set; }
        public List<FeedbackTrendViewModel> MonthlyTrends { get; set; } = new List<FeedbackTrendViewModel>();
        public List<CourseFeedbackViewModel> TopRatedCourses { get; set; } = new List<CourseFeedbackViewModel>();
        public List<CourseFeedbackViewModel> LowRatedCourses { get; set; } = new List<CourseFeedbackViewModel>();
    }

    public class FeedbackTrendViewModel
    {
        public string Month { get; set; }
        public int FeedbackCount { get; set; }
        public double AverageRating { get; set; }
    }

    public class CourseFeedbackViewModel
    {
        public int CourseId { get; set; }
        public string CourseTitle { get; set; }
        public double AverageRating { get; set; }
        public int FeedbackCount { get; set; }
        public string InstructorName { get; set; }
    }
} 