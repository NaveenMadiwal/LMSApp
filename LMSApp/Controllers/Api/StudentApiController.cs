using LMSApp.Data;
using LMSApp.Models;
using LMSApp.Models.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LMSApp.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    public class StudentApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public StudentApiController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllUsers()
        {
            try
            {
                var users = await _context.Users
                    .Select(u => new StudentListViewModel
                    {
                        Id = u.Id,
                        FullName = u.FullName ?? "N/A",
                        Email = u.Email ?? "N/A",
                        UserName = u.UserName ?? "N/A",
                        PhoneNumber = u.PhoneNumber,
                        DateOfBirth = u.DateOfBirth,
                        RoleType = u.RoleType ?? "Student",
                        IsActive = u.IsActive,
                        EnrollmentCount = 0, // Will be calculated separately
                        CreatedAt = DateTime.Now, // Default value since ApplicationUser doesn't have CreatedAt
                        LastLoginDate = null // Default value since ApplicationUser doesn't have LastLoginDate
                    })
                    .ToListAsync();

                // Calculate enrollment counts separately
                var enrollmentCounts = await _context.Enrollments
                    .Where(e => e.IsActive)
                    .GroupBy(e => e.StudentId)
                    .Select(g => new { StudentId = g.Key, Count = g.Count() })
                    .ToListAsync();

                foreach (var user in users)
                {
                    var enrollmentCount = enrollmentCounts.FirstOrDefault(ec => ec.StudentId == user.Id);
                    user.EnrollmentCount = enrollmentCount?.Count ?? 0;
                }

                return Ok(new { success = true, data = users });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Error retrieving users", error = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetUser(string id)
        {
            try
            {
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Id == id);

                if (user == null)
                {
                    return NotFound(new { success = false, message = "User not found" });
                }

                var userRoles = await _userManager.GetRolesAsync(user);
                var roleType = userRoles.FirstOrDefault() ?? user.RoleType ?? "Student";

                // Calculate enrollment count
                var enrollmentCount = await _context.Enrollments
                    .CountAsync(e => e.StudentId == user.Id && e.IsActive);

                var userViewModel = new StudentViewModel
                {
                    Id = user.Id,
                    FullName = user.FullName ?? "",
                    Email = user.Email ?? "",
                    UserName = user.UserName ?? "",
                    PhoneNumber = user.PhoneNumber,
                    DateOfBirth = user.DateOfBirth,
                    RoleType = roleType,
                    IsActive = user.IsActive,
                    EnrollmentCount = enrollmentCount,
                    CreatedAt = DateTime.Now // Default value since ApplicationUser doesn't have CreatedAt
                };

                return Ok(new { success = true, data = userViewModel });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Error retrieving user", error = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateUser([FromBody] StudentViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new { success = false, message = "Invalid data", errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage) });
                }

                // Check if user already exists
                var existingUser = await _userManager.FindByEmailAsync(model.Email);
                if (existingUser != null)
                {
                    return BadRequest(new { success = false, message = "User with this email already exists" });
                }

                var existingUsername = await _userManager.FindByNameAsync(model.UserName);
                if (existingUsername != null)
                {
                    return BadRequest(new { success = false, message = "Username already exists" });
                }

                // Create new user
                var user = new ApplicationUser
                {
                    UserName = model.UserName,
                    Email = model.Email,
                    FullName = model.FullName,
                    PhoneNumber = model.PhoneNumber,
                    DateOfBirth = model.DateOfBirth,
                    RoleType = model.RoleType,
                    IsActive = model.IsActive
                };

                var result = await _userManager.CreateAsync(user, model.Password ?? "DefaultPassword123!");

                if (result.Succeeded)
                {
                    // Assign role
                    var role = model.RoleType;
                    if (!await _roleManager.RoleExistsAsync(role))
                    {
                        await _roleManager.CreateAsync(new IdentityRole(role));
                    }
                    await _userManager.AddToRoleAsync(user, role);

                    return Ok(new { success = true, message = "User created successfully", data = new { user.Id, user.UserName } });
                }
                else
                {
                    return BadRequest(new { success = false, message = "Failed to create user", errors = result.Errors.Select(e => e.Description) });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Error creating user", error = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(string id, [FromBody] StudentViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new { success = false, message = "Invalid data", errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage) });
                }

                var user = await _userManager.FindByIdAsync(id);
                if (user == null)
                {
                    return NotFound(new { success = false, message = "User not found" });
                }

                // Check if email is already taken by another user
                var existingUser = await _userManager.FindByEmailAsync(model.Email);
                if (existingUser != null && existingUser.Id != id)
                {
                    return BadRequest(new { success = false, message = "Email is already taken by another user" });
                }

                // Check if username is already taken by another user
                var existingUsername = await _userManager.FindByNameAsync(model.UserName);
                if (existingUsername != null && existingUsername.Id != id)
                {
                    return BadRequest(new { success = false, message = "Username is already taken by another user" });
                }

                // Update user properties
                user.FullName = model.FullName;
                user.Email = model.Email;
                user.UserName = model.UserName;
                user.PhoneNumber = model.PhoneNumber;
                user.DateOfBirth = model.DateOfBirth;
                user.RoleType = model.RoleType;
                user.IsActive = model.IsActive;

                var result = await _userManager.UpdateAsync(user);

                if (result.Succeeded)
                {
                    // Update role if changed
                    var currentRoles = await _userManager.GetRolesAsync(user);
                    var newRole = model.RoleType;

                    if (!currentRoles.Contains(newRole))
                    {
                        // Remove old roles and add new role
                        await _userManager.RemoveFromRolesAsync(user, currentRoles);
                        
                        if (!await _roleManager.RoleExistsAsync(newRole))
                        {
                            await _roleManager.CreateAsync(new IdentityRole(newRole));
                        }
                        await _userManager.AddToRoleAsync(user, newRole);
                    }

                    return Ok(new { success = true, message = "User updated successfully", data = new { user.Id, user.UserName } });
                }
                else
                {
                    return BadRequest(new { success = false, message = "Failed to update user", errors = result.Errors.Select(e => e.Description) });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Error updating user", error = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(string id)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(id);
                if (user == null)
                {
                    return NotFound(new { success = false, message = "User not found" });
                }

                // Soft delete - set IsActive to false
                user.IsActive = false;
                var result = await _userManager.UpdateAsync(user);

                if (result.Succeeded)
                {
                    return Ok(new { success = true, message = "User deleted successfully" });
                }
                else
                {
                    return BadRequest(new { success = false, message = "Failed to delete user", errors = result.Errors.Select(e => e.Description) });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Error deleting user", error = ex.Message });
            }
        }

        [HttpGet("{userId}/enrollments")]
        public async Task<IActionResult> GetUserEnrollments(string userId)
        {
            try
            {
                var enrollments = await _context.Enrollments
                    .Include(e => e.Student)
                    .Include(e => e.Course)
                    .Where(e => e.StudentId == userId && e.IsActive)
                    .Select(e => new EnrollmentViewModel
                    {
                        Id = e.Id,
                        StudentId = e.StudentId,
                        StudentName = e.Student.FullName ?? "N/A",
                        CourseId = e.CourseId,
                        CourseTitle = e.Course.Title,
                        EnrolledOn = e.EnrolledOn,
                        CompletionStatus = e.CompletionStatus,
                        IsActive = e.IsActive
                    })
                    .ToListAsync();

                return Ok(new { success = true, data = enrollments });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Error retrieving enrollments", error = ex.Message });
            }
        }

        [HttpGet("{userId}/progress")]
        public async Task<IActionResult> GetUserProgress(string userId)
        {
            try
            {
                var enrollments = await _context.Enrollments
                    .Include(e => e.Course)
                    .Where(e => e.StudentId == userId && e.IsActive)
                    .ToListAsync();

                var progressList = enrollments.Select(e => new {
                    CourseTitle = e.Course?.Title ?? "N/A",
                    CompletionStatus = e.CompletionStatus,
                    EnrolledOn = e.EnrolledOn,
                    ProgressPercentage = e.CompletionStatus == "Completed" ? 100 : 0
                }).ToList();

                return Ok(new { success = true, data = progressList });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Error retrieving progress", error = ex.Message });
            }
        }

        [HttpGet("roles")]
        public async Task<IActionResult> GetRoles()
        {
            try
            {
                var roles = await _roleManager.Roles
                    .Select(r => new { r.Id, r.Name })
                    .ToListAsync();

                return Ok(new { success = true, data = roles });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Error retrieving roles", error = ex.Message });
            }
        }
    }
} 