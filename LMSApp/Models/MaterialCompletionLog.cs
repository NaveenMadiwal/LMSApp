using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LMSApp.Models
{
    public class MaterialCompletionLog
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string StudentId { get; set; } = string.Empty;

        [ForeignKey("StudentId")]
        public ApplicationUser Student { get; set; } = null!;

        [Required]
        public int CourseId { get; set; }

        [ForeignKey("CourseId")]
        public Course Course { get; set; } = null!;

        [Required]
        public int MaterialId { get; set; }

        [ForeignKey("MaterialId")]
        public CourseMaterial Material { get; set; } = null!;

        [Required]
        public DateTime AccessedAt { get; set; } = DateTime.Now;

        [Required]
        public string Action { get; set; } = string.Empty; // "Viewed", "Downloaded", "Completed"

        public int? TimeSpentSeconds { get; set; } // For videos or interactive content

        public bool IsCompleted { get; set; } = false;

        public DateTime? CompletedAt { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
} 