using LMSApp.Data;
using LMSApp.Models;
using LMSApp.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Security.Cryptography.X509Certificates;

namespace LMSApp.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly string _baseApiUrl = "https://localhost:7052/api/CategoryApi";


        public AdminController(ApplicationDbContext context, IHttpClientFactory httpClientFactory)
        {
            _context = context;
            _httpClientFactory = httpClientFactory;
        }
        public async Task<IActionResult> Dashboard()
        {
            var model = new AdminDashboardViewModel
            {
                TotalCourses = await _context.Courses.CountAsync(c => c.IsActive),
                TotalEnrollments = await _context.Enrollments.CountAsync(e => e.IsActive),
                TotalStudents = await _context.Users.CountAsync(),
                TotalCategories = await _context.Categories.CountAsync(c => c.IsActive)
            };

            return View(model);
        }

        public async Task<IActionResult> Category()
        {
            var client = _httpClientFactory.CreateClient();
            var response = await client.GetAsync(_baseApiUrl);

            if (!response.IsSuccessStatusCode)
            {
                ViewBag.Error = "Failed to fetch categories";
                return View(new List<Category>());
            }

            var json = await response.Content.ReadAsStringAsync();
            var categories = JsonConvert.DeserializeObject<List<Category>>(json);

            return View(categories);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateCategory(Category model)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Invalid category name.";
                return RedirectToAction("Category");
            } 

            var client = _httpClientFactory.CreateClient();

            var response = await client.PostAsJsonAsync(_baseApiUrl, model);

            if (response.IsSuccessStatusCode)
            {
                TempData["Success"] = "Category added successfully.";
            }
            else
            {
                TempData["Error"] = "Failed to add category.";
            }

            return RedirectToAction("Category");
        }

    }
}
