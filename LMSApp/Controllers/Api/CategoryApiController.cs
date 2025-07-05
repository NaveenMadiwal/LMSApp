using LMSApp.Data;
using LMSApp.Models;
using LMSApp.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LMSApp.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
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
        public async Task<IActionResult> Create([FromBody] CategoryViewModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var category = new Category
            {
                Name = model.Name,
                CreatedAt = DateTime.Now,
                IsActive = true
            };

            _context.Categories.Add(category);
            await _context.SaveChangesAsync();

            return Ok(category);
        }


        // PUT: api/CategoryApi/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] CategoryViewModel model)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null || !category.IsActive)
                return NotFound();

            category.Name = model.Name;
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
