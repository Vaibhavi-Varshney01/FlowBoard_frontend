namespace FlowBoard.Auth.DTOs
{
    public class AuthResponse
    {
        public string Token { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }
        public string AvatarUrl { get; set; } = string.Empty;

        public AuthResponse(string token, string username, string email, string role, string avatarUrl = "")
        {
            Token = token;
            Username = username;
            Email = email;
            Role = role;
            AvatarUrl = avatarUrl;
        }
    }
}