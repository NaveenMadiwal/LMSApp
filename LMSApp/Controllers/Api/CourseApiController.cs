using LMSApp.Data;
using LMSApp.Models;
using Microsoft.AspNetCore.Mvc;

namespace LMSApp.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    public class CourseApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public CourseApiController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult GetAllCourses()
        {
            var course = _context.Courses
                .Where(c => c.IsActive)
                .Select(c => new
                {
                    c.Id,
                    c.Title,
                    c.Description,
                    c.StartDate,
                    c.EndDate
                }).ToList();

            return Ok(course);

        }

        [HttpPost]
        public IActionResult CreateCourse([FromBody] Course course)
        {
            if (!ModelState.IsValid)
            {
                BadRequest(ModelState);

            } 
                course.CreatedAt = DateTime.Now;
                _context.Courses.Add(course);
                _context.SaveChanges();

                return Ok(course); 
        }
    }
}
