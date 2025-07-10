using System.ComponentModel.DataAnnotations;

namespace LMSApp.Models.ViewModels
{
    public class CourseViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Course title is required")]
        [StringLength(100, ErrorMessage = "Title cannot exceed 100 characters")]
        [Display(Name = "Course Title")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Course description is required")]
        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
        [Display(Name = "Description")]
        public string Description { get; set; } = string.Empty;

        [Display(Name = "Start Date")]
        [DataType(DataType.Date)]
        public DateTime? StartDate { get; set; }

        [Display(Name = "End Date")]
        [DataType(DataType.Date)]
        public DateTime? EndDate { get; set; }

        [Display(Name = "Category")]
        public int? CategoryId { get; set; }

        [Display(Name = "Created By")]
        public string CreatedById { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;

        // Navigation properties
        public string CategoryName { get; set; } = string.Empty;
        public string CreatedByName { get; set; } = string.Empty;
        public List<CourseMaterialViewModel> Materials { get; set; } = new List<CourseMaterialViewModel>();
        public int EnrollmentCount { get; set; }
    }

    public class CourseMaterialViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Material title is required")]
        [StringLength(100, ErrorMessage = "Title cannot exceed 100 characters")]
        [Display(Name = "Material Title")]
        public string Title { get; set; } = string.Empty;

        [Display(Name = "File")]
        public IFormFile? File { get; set; }

        [Display(Name = "File Type")]
        public string FileType { get; set; } = string.Empty;

        public string FilePath { get; set; } = string.Empty;
        public int CourseId { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class CourseListViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int? CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public string CreatedById { get; set; } = string.Empty;
        public string CreatedByName { get; set; } = string.Empty;
        public int MaterialCount { get; set; }
        public int EnrollmentCount { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
    }
} 