namespace FlowBoard.Web.ViewModels
{
    public class BoardViewModel
    {
        public int         BoardId     { get; set; }
        public string      Name        { get; set; } = string.Empty;
        public string      Background  { get; set; } = string.Empty;
        public List<object> Lists      { get; set; } = new();
        public List<object> Members    { get; set; } = new();
        public List<object> Labels     { get; set; } = new();
    }
}