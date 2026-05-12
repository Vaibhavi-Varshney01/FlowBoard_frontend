namespace FlowBoard.Auth.DTOs
{
    public class OAuthRequest
    {
        public string Email { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Provider { get; set; } = "GOOGLE";
        public string AvatarUrl { get; set; } = string.Empty;
        public string? Role { get; set; }
    }
}
