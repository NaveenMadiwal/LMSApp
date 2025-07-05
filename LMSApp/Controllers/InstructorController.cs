using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using LMSApp.Models.ViewModels;
using System.Net.Http.Json;
using System.Text.Json;

namespace LMSApp.Controllers
{
    [Authorize(Roles = "Instructor")]
    public class InstructorController : Controller
    {
        // Simple response model for API calls
        private class ApiResponse
        {
            public bool Success { get; set; }
            public object Data { get; set; }
            public string Message { get; set; }
        }

        private readonly IHttpClientFactory _httpClientFactory;

        public InstructorController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public IActionResult Dashboard()
        {
            return View();
        }

        public IActionResult Courses()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetMyCourses()
        {
            try
            {
                var instructorId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(instructorId))
                {
                    return Json(new { success = false, message = "User not authenticated" });
                }

                var client = _httpClientFactory.CreateClient();
                var apiUrl = $"https://localhost:7052/api/CourseApi/creator/{instructorId}";
                
                // Debug: Log the API call
                System.Diagnostics.Debug.WriteLine($"Calling API: {apiUrl}");
                System.Diagnostics.Debug.WriteLine($"Instructor ID: {instructorId}");
                
                var response = await client.GetAsync(apiUrl);

                if (response.IsSuccessStatusCode)
                {
                    var jsonString = await response.Content.ReadAsStringAsync();
                    System.Diagnostics.Debug.WriteLine($"API Response JSON: {jsonString}");
                    
                    try
                    {
                        var apiResponse = JsonSerializer.Deserialize<ApiResponse>(jsonString, new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        });
                        
                        if (apiResponse?.Success == true)
                        {
                            return Json(new { success = true, data = apiResponse.Data });
                        }
                        else
                        {
                            return Json(new { success = false, message = apiResponse?.Message ?? "Unknown error" });
                        }
                    }
                    catch (JsonException ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"JSON Deserialization Error: {ex.Message}");
                        return Json(new { success = false, message = "Invalid response format from API" });
                    }
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    System.Diagnostics.Debug.WriteLine($"API Error: {response.StatusCode} - {errorContent}");
                    return Json(new { success = false, message = $"Failed to load courses. Status: {response.StatusCode}" });
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Exception: {ex.Message}");
                return Json(new { success = false, message = "Error loading courses", error = ex.Message });
            }
        }

        public IActionResult Enrollments()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetCourseEnrollments(int courseId)
        {
            try
            {
                var instructorId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                var client = _httpClientFactory.CreateClient();
                
                // First, verify the course belongs to the instructor
                var courseResponse = await client.GetAsync($"https://localhost:7052/api/CourseApi/{courseId}");
                if (!courseResponse.IsSuccessStatusCode)
                {
                    return Json(new { success = false, message = "Course not found" });
                }
                
                var courseResult = await courseResponse.Content.ReadFromJsonAsync<dynamic>();
                if (courseResult?.success != true || courseResult.data["createdById"]?.ToString() != instructorId)
                {
                    return Json(new { success = false, message = "Access denied to this course" });
                }
                
                // Get enrollments for this course
                var enrollmentsResponse = await client.GetAsync($"https://localhost:7052/api/EnrollmentApi/course/{courseId}");
                if (enrollmentsResponse.IsSuccessStatusCode)
                {
                    var enrollmentsResult = await enrollmentsResponse.Content.ReadFromJsonAsync<dynamic>();
                    if (enrollmentsResult?.success == true)
                    {
                        return Json(new { 
                            success = true, 
                            data = enrollmentsResult.data,
                            courseTitle = courseResult.data["title"]?.ToString()
                        });
                    }
                }
                
                return Json(new { success = false, message = "Failed to load enrollments" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error loading enrollments", error = ex.Message });
            }
        }

        public IActionResult Profile()
        {
            return View();
        }

        // Course Management Actions
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateCourse(CourseViewModel model)
        {
            // Debug: Log the received model and ModelState
            var modelStateErrors = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .ToList();
            
            var receivedData = new
            {
                Title = model?.Title,
                Description = model?.Description,
                CategoryId = model?.CategoryId,
                StartDate = model?.StartDate,
                EndDate = model?.EndDate,
                IsActive = model?.IsActive
            };
            
            if (!ModelState.IsValid)
            {
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { 
                        success = false, 
                        message = "Invalid course data.", 
                        errors = modelStateErrors,
                        receivedData = receivedData
                    });
                }
                TempData["Error"] = "Invalid course data: " + string.Join(", ", modelStateErrors);
                return RedirectToAction("Courses");
            }

            // Set the CreatedById to the current user's ID
            model.CreatedById = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

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

            return RedirectToAction("Courses");
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
                return RedirectToAction("Courses");
            }

            // Set the CreatedById to the current user's ID
            model.CreatedById = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

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

            return RedirectToAction("Courses");
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

            return RedirectToAction("Courses");
        }
    }
}
