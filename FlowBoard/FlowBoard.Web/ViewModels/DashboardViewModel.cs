using FlowBoard.Web.DTOs;

namespace FlowBoard.Web.ViewModels
{
    public class DashboardViewModel
    {
        public string              UserName        { get; set; } = string.Empty;
        public List<object>        Workspaces      { get; set; } = new();
        public List<object>        RecentBoards    { get; set; } = new();
        public int                 UnreadCount     { get; set; }
    }
}