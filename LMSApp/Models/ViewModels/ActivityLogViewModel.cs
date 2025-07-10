namespace LMSApp.Models.ViewModels
{
    public class ActivityLogViewModel
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string Action { get; set; }
        public string Description { get; set; }
        public string EntityType { get; set; }
        public string EntityId { get; set; }
        public DateTime Timestamp { get; set; }
        public string IpAddress { get; set; }
        public string UserAgent { get; set; }
        public ActivitySeverity Severity { get; set; }
    }

    public enum ActivitySeverity
    {
        Low,
        Medium,
        High,
        Critical
    }
} 