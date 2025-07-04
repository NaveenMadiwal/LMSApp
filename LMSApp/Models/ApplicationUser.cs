using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace LMSApp.Models
{
    public class ApplicationUser : IdentityUser
    {
        [Required]
        public string? FullName { get; set; }

        public DateTime? DateOfBirth { get; set; }

        // Just for UI filtering (not used for security logic)
        public string? RoleType { get; set; }

        public bool IsActive { get; set; } = true;

    }
}
