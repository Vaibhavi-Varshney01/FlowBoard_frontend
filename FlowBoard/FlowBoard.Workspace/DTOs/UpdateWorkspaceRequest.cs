namespace FlowBoard.Workspace.DTOs
{
    public record UpdateWorkspaceRequest
    {
        public string  Name        { get; init; } = string.Empty;
        public string? Description { get; init; }
        public string  Visibility  { get; init; } = string.Empty;
        public string? LogoUrl     { get; init; }
    }
}