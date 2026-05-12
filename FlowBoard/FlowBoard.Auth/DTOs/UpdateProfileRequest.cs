namespace FlowBoard.Auth.DTOs
{
    public class UpdateProfileRequest
    {
        public string FullName { get; set; } = string.Empty;

        public string Username { get; set; } = string.Empty;

        public string AvatarUrl { get; set; } = string.Empty;
    }
}