namespace FlowBoard.Workspace.DTOs
{
    public record CreateWorkspaceRequest
    {
        public string  Name        { get; init; } = string.Empty;
        public string? Description { get; init; }
        public string  OwnerId     { get; init; } = string.Empty;
        public string  Visibility  { get; init; } = string.Empty;
        public string? LogoUrl     { get; init; }
    }
}