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

        public async Task<IActionResult> Course()
        {
            var client = _httpClientFactory.CreateClient();
            var response = await client.GetAsync("https://localhost:7052/api/CourseApi");

            if (!response.IsSuccessStatusCode)
            {
                ViewBag.Error = "Failed to fetch courses";
                return View(new List<CourseListViewModel>());
            }

            var json = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<dynamic>(json);
            var courses = JsonConvert.DeserializeObject<List<CourseListViewModel>>(result.data.ToString());

            return View(courses);
        }

        public async Task<IActionResult> Student()
        {
            var client = _httpClientFactory.CreateClient();
            var response = await client.GetAsync("https://localhost:7052/api/StudentApi");

            if (!response.IsSuccessStatusCode)
            {
                ViewBag.Error = "Failed to fetch users";
                return View(new List<StudentListViewModel>());
            }

            var json = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<dynamic>(json);
            var users = JsonConvert.DeserializeObject<List<StudentListViewModel>>(result.data.ToString());

            return View(users);
        }

        public async Task<IActionResult> StudentEnrollments()
        {
            var client = _httpClientFactory.CreateClient();
            var response = await client.GetAsync("https://localhost:7052/api/StudentApi");

            if (!response.IsSuccessStatusCode)
            {
                ViewBag.Error = "Failed to fetch students";
                return View(new List<StudentListViewModel>());
            }

            var json = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<dynamic>(json);
            var allUsers = JsonConvert.DeserializeObject<List<StudentListViewModel>>(result.data.ToString());
            
            // Filter only students - use foreach to avoid dynamic dispatch issue
            var students = new List<StudentListViewModel>();
            foreach (var user in allUsers)
            {
                if (user.RoleType == "Student")
                {
                    students.Add(user);
                }
            }

            return View(students);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateCategory(CategoryViewModel model)
        {
            if (!ModelState.IsValid)
            {
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = false, message = "Invalid category name." });
                }
                TempData["Error"] = "Invalid category name.";
                return RedirectToAction("Category");
            } 

            var client = _httpClientFactory.CreateClient();

            var response = await client.PostAsJsonAsync(_baseApiUrl, model);

            if (response.IsSuccessStatusCode)
            {
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = true, message = "Category added successfully." });
                }
                TempData["Success"] = "Category added successfully.";
            }
            else
            {
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = false, message = "Failed to add category." });
                }
                TempData["Error"] = "Failed to add category.";
            }

            return RedirectToAction("Category");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateCategory(CategoryViewModel model)
        {
            if (!ModelState.IsValid)
            {
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = false, message = "Invalid data." });
                }
                TempData["Error"] = "Invalid data.";
                return RedirectToAction("Category");
            }

            var client = _httpClientFactory.CreateClient();
            var response = await client.PutAsJsonAsync($"{_baseApiUrl}/{model.Id}", model);

            if (response.IsSuccessStatusCode)
            {
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = true, message = "Category updated successfully.", categoryName = model.Name });
                }
                TempData["Success"] = "Category updated successfully.";
            }
            else
            {
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = false, message = "Failed to update category." });
                }
                TempData["Error"] = "Failed to update category.";
            }

            return RedirectToAction("Category");
        }

        [HttpPost]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            var client = _httpClientFactory.CreateClient();
            var response = await client.DeleteAsync($"{_baseApiUrl}/{id}");

            if (response.IsSuccessStatusCode)
            {
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = true, message = "Category deleted successfully." });
                }
                TempData["Success"] = "Category deleted (inactivated).";
            }
            else
            {
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = false, message = "Failed to delete category." });
                }
                TempData["Error"] = "Failed to delete category.";
            }

            return RedirectToAction("Category");
        }

        // Course Management Actions
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateCourse(CourseViewModel model)
        {
            if (!ModelState.IsValid)
            {
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = false, message = "Invalid course data." });
                }
                TempData["Error"] = "Invalid course data.";
                return RedirectToAction("Course");
            }

            // Set the CreatedById to the current user's ID if not provided
            if (string.IsNullOrEmpty(model.CreatedById))
            {
                model.CreatedById = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            }

            var client = _httpClientFactory.CreateClient();
            var response = await client.PostAsJsonAsync("https://localhost:7052/api/CourseApi", model);

            if (response.IsSuccessStatusCode)
            {
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = true, message = "Course created successfully." });
                }
                TempData["Success"] = "Course created successfully.";
            }
            else
            {
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = false, message = "Failed to create course." });
                }
                TempData["Error"] = "Failed to create course.";
            }

            return RedirectToAction("Course");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateCourse(CourseViewModel model)
        {
            if (!ModelState.IsValid)
            {
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = false, message = "Invalid course data." });
                }
                TempData["Error"] = "Invalid course data.";
                return RedirectToAction("Course");
            }

            // Set the CreatedById to the current user's ID if not provided
            if (string.IsNullOrEmpty(model.CreatedById))
            {
                model.CreatedById = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            }

            var client = _httpClientFactory.CreateClient();
            var response = await client.PutAsJsonAsync($"https://localhost:7052/api/CourseApi/{model.Id}", model);

            if (response.IsSuccessStatusCode)
            {
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = true, message = "Course updated successfully.", courseTitle = model.Title });
                }
                TempData["Success"] = "Course updated successfully.";
            }
            else
            {
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = false, message = "Failed to update course." });
                }
                TempData["Error"] = "Failed to update course.";
            }

            return RedirectToAction("Course");
        }

        [HttpPost]
        public async Task<IActionResult> DeleteCourse(int id)
        {
            var client = _httpClientFactory.CreateClient();
            var response = await client.DeleteAsync($"https://localhost:7052/api/CourseApi/{id}");

            if (response.IsSuccessStatusCode)
            {
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = true, message = "Course deleted successfully." });
                }
                TempData["Success"] = "Course deleted successfully.";
            }
            else
            {
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = false, message = "Failed to delete course." });
                }
                TempData["Error"] = "Failed to delete course.";
            }

            return RedirectToAction("Course");
        }

        // Student Management Actions
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateStudent(StudentViewModel model)
        {
            if (!ModelState.IsValid)
            {
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = false, message = "Invalid user data." });
                }
                TempData["Error"] = "Invalid user data.";
                return RedirectToAction("Student");
            }

            var client = _httpClientFactory.CreateClient();
            var response = await client.PostAsJsonAsync("https://localhost:7052/api/StudentApi", model);

            if (response.IsSuccessStatusCode)
            {
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = true, message = "User created successfully." });
                }
                TempData["Success"] = "User created successfully.";
            }
            else
            {
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = false, message = "Failed to create user." });
                }
                TempData["Error"] = "Failed to create user.";
            }

            return RedirectToAction("Student");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStudent(StudentViewModel model)
        {
            if (!ModelState.IsValid)
            {
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = false, message = "Invalid user data." });
                }
                TempData["Error"] = "Invalid user data.";
                return RedirectToAction("Student");
            }

            var client = _httpClientFactory.CreateClient();
            var response = await client.PutAsJsonAsync($"https://localhost:7052/api/StudentApi/{model.Id}", model);

            if (response.IsSuccessStatusCode)
            {
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = true, message = "User updated successfully.", userName = model.UserName });
                }
                TempData["Success"] = "User updated successfully.";
            }
            else
            {
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = false, message = "Failed to update user." });
                }
                TempData["Error"] = "Failed to update user.";
            }

            return RedirectToAction("Student");
        }

        [HttpPost]
        public async Task<IActionResult> DeleteStudent(string id)
        {
            var client = _httpClientFactory.CreateClient();
            var response = await client.DeleteAsync($"https://localhost:7052/api/StudentApi/{id}");

            if (response.IsSuccessStatusCode)
            {
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = true, message = "User deleted successfully." });
                }
                TempData["Success"] = "User deleted successfully.";
            }
            else
            {
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = false, message = "Failed to delete user." });
                }
                TempData["Error"] = "Failed to delete user.";
            }

            return RedirectToAction("Student");
        }

    }
}
