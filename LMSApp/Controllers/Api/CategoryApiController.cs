using LMSApp.Data;
using LMSApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LMSApp.Controllers.Api
{
    public class CategoryApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public CategoryApiController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/CategoryApi
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var categories = await _context.Categories
                .Where(c => c.IsActive)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();

            return Ok(categories);
        }

        // POST: api/CategoryApi
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Category category)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            category.CreatedAt = DateTime.Now;
            category.IsActive = true;
            _context.Categories.Add(category);
            await _context.SaveChangesAsync();

            return Ok(category);
        }

        // PUT: api/CategoryApi/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] Category updated)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null || !category.IsActive)
                return NotFound();

            category.Name = updated.Name;
            category.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();
            return Ok(category);
        }

        // DELETE (Soft Delete): api/CategoryApi/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Inactivate(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null || !category.IsActive)
                return NotFound();

            category.IsActive = false;
            category.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();
            return Ok(new { message = "Category deactivated" });
        }

    }
}
