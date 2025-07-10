using System.ComponentModel.DataAnnotations;

namespace LMSApp.Models.ViewModels
{
    public class CourseDetailViewModel
    {
        public int CourseId { get; set; }
        
        [Display(Name = "Course Title")]
        public string Title { get; set; } = string.Empty;
        
        [Display(Name = "Description")]
        public string Description { get; set; } = string.Empty;
        
        [Display(Name = "Instructor")]
        public string InstructorName { get; set; } = string.Empty;
        
        [Display(Name = "Start Date")]
        public DateTime? StartDate { get; set; }
        
        [Display(Name = "End Date")]
        public DateTime? EndDate { get; set; }
        
        [Display(Name = "Enrollment Count")]
        public int EnrollmentCount { get; set; }
        
        [Display(Name = "Material Count")]
        public int MaterialCount { get; set; }
        
        [Display(Name = "Progress Percentage")]
        public int ProgressPercentage { get; set; }
        
        [Display(Name = "Completion Status")]
        public string CompletionStatus { get; set; } = string.Empty;
        
        [Display(Name = "Enrolled Date")]
        public DateTime EnrolledDate { get; set; }
        
        [Display(Name = "Materials Completed")]
        public int MaterialsCompleted { get; set; }
        
        [Display(Name = "Total Materials")]
        public int TotalMaterials { get; set; }
        
        [Display(Name = "Videos Watched")]
        public int VideosWatched { get; set; }
        
        [Display(Name = "Documents Read")]
        public int DocumentsRead { get; set; }
        
        [Display(Name = "Last Accessed")]
        public DateTime? LastAccessed { get; set; }
        
        [Display(Name = "Has Submitted Feedback")]
        public bool HasSubmittedFeedback { get; set; }
        
        [Display(Name = "Student Rating")]
        public int StudentRating { get; set; }
        
        [Display(Name = "Student Feedback")]
        public string StudentFeedback { get; set; } = string.Empty;
        
        [Display(Name = "Average Rating")]
        public double AverageRating { get; set; }
        
        [Display(Name = "Total Ratings")]
        public int TotalRatings { get; set; }

        [Display(Name = "Can Submit Feedback")]
        public bool CanSubmitFeedback { get; set; }
    }
} 