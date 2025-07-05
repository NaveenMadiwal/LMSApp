using LMSApp.Data;
using LMSApp.Models;
using LMSApp.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LMSApp.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    public class CourseApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public CourseApiController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllCourses()
        {
            try
            {
                var courses = await _context.Courses
                    .Where(c => c.IsActive)
                    .Include(c => c.Category)
                    .Include(c => c.CreatedBy)
                    .Include(c => c.CourseMaterials)
                    .Include(c => c.Enrollments)
                    .Select(c => new CourseListViewModel
                    {
                        Id = c.Id,
                        Title = c.Title,
                        Description = c.Description,
                        StartDate = c.StartDate,
                        EndDate = c.EndDate,
                        CategoryId = c.CategoryId,
                        CategoryName = c.Category != null ? c.Category.Name : "Uncategorized",
                        CreatedByName = c.CreatedBy != null ? c.CreatedBy.UserName : "Unknown",
                        MaterialCount = c.CourseMaterials != null ? c.CourseMaterials.Count : 0,
                        EnrollmentCount = c.Enrollments != null ? c.Enrollments.Count : 0,
                        IsActive = c.IsActive,
                        CreatedAt = c.CreatedAt
                    })
                    .ToListAsync();

                return Ok(new { success = true, data = courses });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Error retrieving courses", error = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetCourse(int id)
        {
            try
            {
                var course = await _context.Courses
                    .Include(c => c.Category)
                    .Include(c => c.CreatedBy)
                    .Include(c => c.CourseMaterials)
                    .Include(c => c.Enrollments)
                    .FirstOrDefaultAsync(c => c.Id == id && c.IsActive);

                if (course == null)
                {
                    return NotFound(new { success = false, message = "Course not found" });
                }

                var courseViewModel = new CourseViewModel
                {
                    Id = course.Id,
                    Title = course.Title,
                    Description = course.Description,
                    StartDate = course.StartDate,
                    EndDate = course.EndDate,
                    CategoryId = course.CategoryId,
                    CategoryName = course.Category?.Name,
                    CreatedById = course.CreatedById,
                    CreatedByName = course.CreatedBy?.UserName,
                    IsActive = course.IsActive,
                    EnrollmentCount = course.Enrollments?.Count ?? 0,
                    Materials = course.CourseMaterials?.Select(m => new CourseMaterialViewModel
                    {
                        Id = m.Id,
                        Title = m.Title,
                        FileType = m.FileType,
                        FilePath = m.FilePath,
                        CourseId = m.CourseId,
                        CreatedAt = m.CreatedAt
                    }).ToList() ?? new List<CourseMaterialViewModel>()
                };

                return Ok(new { success = true, data = courseViewModel });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Error retrieving course", error = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateCourse([FromBody] CourseViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new { success = false, message = "Invalid data", errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage) });
                }

                // Only validate dates if both are provided
                if (model.StartDate.HasValue && model.EndDate.HasValue && model.StartDate >= model.EndDate)
                {
                    return BadRequest(new { success = false, message = "End date must be after start date" });
                }

                var course = new Course
                {
                    Title = model.Title,
                    Description = model.Description,
                    StartDate = model.StartDate,
                    EndDate = model.EndDate,
                    CategoryId = model.CategoryId,
                    CreatedById = model.CreatedById,
                    IsActive = model.IsActive,
                    CreatedAt = DateTime.Now
                };

                _context.Courses.Add(course);
                await _context.SaveChangesAsync();

                return Ok(new { success = true, message = "Course created successfully", data = new { course.Id, course.Title } });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Error creating course", error = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCourse(int id, [FromBody] CourseViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new { success = false, message = "Invalid data", errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage) });
                }

                var course = await _context.Courses.FindAsync(id);
                if (course == null)
                {
                    return NotFound(new { success = false, message = "Course not found" });
                }

                // Only validate dates if both are provided
                if (model.StartDate.HasValue && model.EndDate.HasValue && model.StartDate >= model.EndDate)
                {
                    return BadRequest(new { success = false, message = "End date must be after start date" });
                }

                course.Title = model.Title;
                course.Description = model.Description;
                course.StartDate = model.StartDate;
                course.EndDate = model.EndDate;
                course.CategoryId = model.CategoryId;
                course.IsActive = model.IsActive;
                course.UpdatedAt = DateTime.Now;

                await _context.SaveChangesAsync();

                return Ok(new { success = true, message = "Course updated successfully", data = new { course.Id, course.Title } });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Error updating course", error = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCourse(int id)
        {
            try
            {
                var course = await _context.Courses.FindAsync(id);
                if (course == null)
                {
                    return NotFound(new { success = false, message = "Course not found" });
                }

                // Soft delete - set IsActive to false
                course.IsActive = false;
                course.UpdatedAt = DateTime.Now;

                await _context.SaveChangesAsync();

                return Ok(new { success = true, message = "Course deleted successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Error deleting course", error = ex.Message });
            }
        }

        [HttpPost("{courseId}/materials")]
        public async Task<IActionResult> AddCourseMaterial(int courseId, [FromForm] CourseMaterialViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new { success = false, message = "Invalid data", errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage) });
                }

                var course = await _context.Courses.FindAsync(courseId);
                if (course == null)
                {
                    return NotFound(new { success = false, message = "Course not found" });
                }

                if (model.File == null || model.File.Length == 0)
                {
                    return BadRequest(new { success = false, message = "File is required" });
                }

                // Create uploads directory if it doesn't exist
                var uploadsPath = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "course-materials");
                if (!Directory.Exists(uploadsPath))
                {
                    Directory.CreateDirectory(uploadsPath);
                }

                // Generate unique filename
                var fileName = $"{Guid.NewGuid()}_{model.File.FileName}";
                var filePath = Path.Combine(uploadsPath, fileName);

                // Save file
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await model.File.CopyToAsync(stream);
                }

                var material = new CourseMaterial
                {
                    CourseId = courseId,
                    Title = model.Title,
                    FilePath = $"/uploads/course-materials/{fileName}",
                    FileType = Path.GetExtension(model.File.FileName).ToLowerInvariant(),
                    CreatedAt = DateTime.Now
                };

                _context.CourseMaterials.Add(material);
                await _context.SaveChangesAsync();

                return Ok(new { success = true, message = "Material added successfully", data = new { material.Id, material.Title } });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Error adding material", error = ex.Message });
            }
        }

        [HttpDelete("materials/{materialId}")]
        public async Task<IActionResult> DeleteCourseMaterial(int materialId)
        {
            try
            {
                var material = await _context.CourseMaterials.FindAsync(materialId);
                if (material == null)
                {
                    return NotFound(new { success = false, message = "Material not found" });
                }

                // Delete physical file
                var filePath = Path.Combine(_webHostEnvironment.WebRootPath, material.FilePath.TrimStart('/'));
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }

                _context.CourseMaterials.Remove(material);
                await _context.SaveChangesAsync();

                return Ok(new { success = true, message = "Material deleted successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Error deleting material", error = ex.Message });
            }
        }

        [HttpGet("categories")]
        public async Task<IActionResult> GetCategories()
        {
            try
            {
                var categories = await _context.Categories
                    .Where(c => c.IsActive)
                    .Select(c => new { c.Id, c.Name })
                    .ToListAsync();

                return Ok(new { success = true, data = categories });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Error retrieving categories", error = ex.Message });
            }
        }

        [HttpGet("creator/{creatorId}")]
        public async Task<IActionResult> GetCoursesByCreator(string creatorId)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"GetCoursesByCreator called with creatorId: {creatorId}");
                
                var courses = await _context.Courses
                    .Where(c => c.CreatedById == creatorId && c.IsActive)
                    .Include(c => c.Category)
                    .Include(c => c.CreatedBy)
                    .Include(c => c.CourseMaterials)
                    .Include(c => c.Enrollments)
                    .Select(c => new CourseListViewModel
                    {
                        Id = c.Id,
                        Title = c.Title,
                        Description = c.Description,
                        StartDate = c.StartDate,
                        EndDate = c.EndDate,
                        CategoryId = c.CategoryId,
                        CategoryName = c.Category != null ? c.Category.Name : "Uncategorized",
                        CreatedByName = c.CreatedBy != null ? c.CreatedBy.UserName : "Unknown",
                        MaterialCount = c.CourseMaterials != null ? c.CourseMaterials.Count : 0,
                        EnrollmentCount = c.Enrollments != null ? c.Enrollments.Count : 0,
                        IsActive = c.IsActive,
                        CreatedAt = c.CreatedAt
                    })
                    .ToListAsync();

                System.Diagnostics.Debug.WriteLine($"Found {courses.Count} courses for creator {creatorId}");
                return Ok(new { success = true, data = courses });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in GetCoursesByCreator: {ex.Message}");
                return StatusCode(500, new { success = false, message = "Error retrieving courses", error = ex.Message });
            }
        }
    }
}
