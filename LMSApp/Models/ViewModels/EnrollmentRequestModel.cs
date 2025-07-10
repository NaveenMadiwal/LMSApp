using System.ComponentModel.DataAnnotations;

namespace LMSApp.Models.ViewModels
{
    public class EnrollmentRequestModel
    {
        [Required]
        public int CourseId { get; set; }
        
        [Required]
        public string StudentId { get; set; } = string.Empty;
    }
} 