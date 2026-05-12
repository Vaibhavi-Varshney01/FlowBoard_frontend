namespace FlowBoard.Web.DTOs
{
    public class CreateCardDto
    {
        public int     ListId      { get; set; }
        public int     BoardId     { get; set; }
        public string  Title       { get; set; } = string.Empty;
        public string  Description { get; set; } = string.Empty;
        public string  Priority    { get; set; } = "LOW";
        public DateOnly? DueDate   { get; set; }
    }
}