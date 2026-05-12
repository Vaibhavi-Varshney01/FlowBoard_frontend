using Microsoft.AspNetCore.Identity;

namespace FlowBoard.Auth.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string FullName { get; set; } = string.Empty;
        public string Role { get; set; } = "MEMBER";
        public string? AvatarUrl { get; set; } = string.Empty;
        public string? Provider { get; set; } = "LOCAL"; // LOCAL | GOOGLE | GITHUB
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}