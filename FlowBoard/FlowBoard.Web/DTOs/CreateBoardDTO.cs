namespace FlowBoard.Web.DTOs
{
    public class CreateBoardDto
    {
        public int     WorkspaceId  { get; set; }
        public string  Name         { get; set; } = string.Empty;
        public string? Description  { get; set; }
        public string? Background   { get; set; }
        public string  Visibility   { get; set; } = "PRIVATE";
    }
}