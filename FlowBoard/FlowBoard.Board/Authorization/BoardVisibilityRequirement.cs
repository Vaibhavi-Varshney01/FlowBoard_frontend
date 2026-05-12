using Microsoft.AspNetCore.Authorization;

namespace FlowBoard.Board.Authorization
{
    /// <summary>
    /// Requirement: the board must be PUBLIC, or the requesting user must be an authenticated member.
    /// </summary>
    public class BoardVisibilityRequirement : IAuthorizationRequirement { }
}