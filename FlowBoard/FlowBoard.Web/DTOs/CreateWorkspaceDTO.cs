namespace FlowBoard.Web.DTOs
{
    public class CreateWorkspaceDto
    {
        public string Name        { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Visibility  { get; set; } = "PRIVATE";
    }
}