using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LMSApp.Data;
using LMSApp.Models;
using LMSApp.Models.ViewModels;
using System.Security.Claims;

namespace LMSApp.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Student")]
    public class FeedbackApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public FeedbackApiController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> PostFeedback([FromBody] FeedbackViewModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("User not authenticated");
            }

            // Verify student is enrolled in this course
            var enrollment = await _context.Enrollments
                .FirstOrDefaultAsync(e => e.StudentId == userId && e.CourseId == model.CourseId && e.IsActive);

            if (enrollment == null)
            {
                return BadRequest("You can only provide feedback for courses you are enrolled in");
            }

            // Check if feedback already exists for this course
            var existingFeedback = await _context.Feedbacks
                .FirstOrDefaultAsync(f => f.StudentId == userId && f.CourseId == model.CourseId);

            if (existingFeedback != null)
            {
                return BadRequest("You have already provided feedback for this course");
            }

            var feedback = new Feedback
            {
                CourseId = model.CourseId,
                StudentId = userId,
                Comment = model.Comments,
                Rating = model.Rating,
                SubmittedAt = DateTime.Now,
                CreatedAt = DateTime.Now
            };

            _context.Feedbacks.Add(feedback);
            await _context.SaveChangesAsync();

            return Ok(new { success = true, message = "Feedback submitted successfully." });
        }

        [HttpGet("history")]
        public async Task<IActionResult> GetFeedbackHistory()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("User not authenticated");
            }

            var feedbackHistory = await _context.Feedbacks
                .Where(f => f.StudentId == userId)
                .Include(f => f.Course)
                .OrderByDescending(f => f.SubmittedAt)
                .Select(f => new
                {
                    courseId = f.CourseId,
                    courseTitle = f.Course.Title,
                    rating = f.Rating,
                    comments = f.Comment,
                    submittedAt = f.SubmittedAt
                })
                .ToListAsync();

            return Ok(new { success = true, feedback = feedbackHistory });
        }

        [HttpGet("course/{courseId}")]
        public async Task<IActionResult> GetCourseFeedback(int courseId)
        {
            var feedback = await _context.Feedbacks
                .Where(f => f.CourseId == courseId)
                .Include(f => f.Student)
                .OrderByDescending(f => f.SubmittedAt)
                .Select(f => new
                {
                    studentName = f.Student.UserName,
                    Rating = f.Rating,
                    comments = f.Comment,
                    submittedAt = f.SubmittedAt
                })
                .ToListAsync();

            var averageRating = feedback.Any() ? feedback.Average(f => f.Rating) : 0;
            var totalFeedback = feedback.Count;

            return Ok(new 
            { 
                success = true, 
                feedback = feedback,
                averageRating = Math.Round(averageRating, 1),
                totalFeedback = totalFeedback
            });
        }
    }
} 