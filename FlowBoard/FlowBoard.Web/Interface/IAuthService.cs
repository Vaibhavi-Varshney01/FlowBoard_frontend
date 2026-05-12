using FlowBoard.Web.DTOs;

namespace FlowBoard.Web.Services
{
    public interface IAuthService
    {
        Task<string?> LoginAsync(LoginDto dto);
        Task<bool>    RegisterAsync(RegisterDto dto);
        Task          LogoutAsync(HttpContext context);
        Task<object?> GetProfileAsync(string token);
        Task<bool>    UpdateProfileAsync(string token, object model);
        Task<bool>    ChangePasswordAsync(string token, int userId, string newPassword);
        Task<List<object>> SearchUsersAsync(string query);
        Task<bool>    DeactivateAccountAsync(string token, int userId);
    }
}