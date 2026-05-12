using FlowBoard.Auth.DTOs;
using FlowBoard.Auth.Models;

namespace FlowBoard.Auth.Services
{
    public interface IAuthService
    {
        Task<AuthResponse> Register(RegisterRequest request);
        Task<AuthResponse> Login(string email, string password, string? requestedRole = null);

        Task<bool> ValidateToken(string token);
        Task<string> RefreshToken(string token);

        Task<ApplicationUser> GetUserByEmail(string email);
        Task<ApplicationUser> GetUserById(string userId);

        Task<ApplicationUser> UpdateProfile(string userId, UpdateProfileRequest request); 
        Task ChangePassword(ChangePasswordRequest request);

        Task DeactivateAccount(string userId);

        Task<List<ApplicationUser>> SearchUsers(string query);
        Task<AuthResponse> HandleOAuthLogin(string email, string fullName, string provider, string avatarUrl, string? requestedRole = null);
        Task<List<ApplicationUser>> GetAllUsers();
        Task SuspendUser(string userId);
        Task ReactivateUser(string userId);
        Task DeleteUser(string userId);
    }
}