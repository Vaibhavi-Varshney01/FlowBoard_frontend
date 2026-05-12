namespace FlowBoard.Web.ViewModels
{
    public class CardViewModel
    {
        public int         CardId      { get; set; }
        public string      Title       { get; set; } = string.Empty;
        public string      Description { get; set; } = string.Empty;
        public string      Priority    { get; set; } = string.Empty;
        public string      Status      { get; set; } = string.Empty;
        public DateOnly?   DueDate     { get; set; }
        public List<object> Comments   { get; set; } = new();
        public List<object> Checklists { get; set; } = new();
        public List<object> Attachments{ get; set; } = new();
        public List<object> Labels     { get; set; } = new();
    }
}