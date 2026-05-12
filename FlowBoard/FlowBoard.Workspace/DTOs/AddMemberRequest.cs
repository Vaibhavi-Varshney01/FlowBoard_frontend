namespace FlowBoard.Workspace.DTOs
{
    public record AddMemberRequest
    {
        public string UserId { get; init; } = string.Empty;
        public string Role   { get; init; } = string.Empty;
    }
}