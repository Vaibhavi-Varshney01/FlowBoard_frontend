using FlowBoard.Workspace.DTOs;
using FlowBoard.Workspace.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FlowBoard.Workspace.Controllers
{
    /// <summary>
    /// Manages workspaces and workspace membership.
    /// All endpoints except GET /{id} and GET /public require a valid JWT Bearer token.
    /// </summary>
    [ApiController]
    [Route("api/workspaces")]
    public class WorkspaceController : ControllerBase
    {
        private readonly IWorkspaceService _wsService;

        public WorkspaceController(IWorkspaceService wsService)
        {
            _wsService = wsService;
        }

        /// <summary>Returns all workspaces the current authenticated user is a member of.</summary>
        [HttpGet]
        [Authorize]
        [ProducesResponseType(typeof(List<FlowBoard.Workspace.Models.Workspace>), 200)]
        public async Task<IActionResult> GetMyWorkspaces()
        {
            try
            {
                var userId = GetCurrentUserId();
                var workspaces = await _wsService.GetByMember(userId);
                return Ok(workspaces);
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized();
            }
        }

        private string GetCurrentUserId()
        {
            var claim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)
                        ?? User.FindFirst("sub");

            if (claim == null)
                throw new UnauthorizedAccessException("User identity claim not found in token.");

            return claim.Value;
        }

        // ─────────────────────────────────────────────
        // WORKSPACE CRUD
        // ─────────────────────────────────────────────

        /// <summary>Creates a new workspace. The authenticated user becomes the owner and is automatically added as an ADMIN member.</summary>
        /// <param name="request">Workspace details.</param>
        /// <response code="201">Workspace created successfully.</response>
        /// <response code="409">A workspace with this name already exists for the owner.</response>
        [HttpPost]
        [Authorize]
        [ProducesResponseType(typeof(FlowBoard.Workspace.Models.Workspace), 201)]
        [ProducesResponseType(typeof(object), 409)]
        public async Task<IActionResult> Create([FromBody] CreateWorkspaceRequest request)
        {
            try
            {
                var userId = GetCurrentUserId();
                // We create a new request with the owner ID from the token for security
                var requestWithOwner = request with { OwnerId = userId };
                
                var workspace = await _wsService.CreateWorkspace(requestWithOwner);
                return CreatedAtAction(nameof(GetById),
                    new { id = workspace.WorkspaceId }, workspace);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
        }

        /// <summary>Gets a workspace by ID. Returns 403 if the workspace is PRIVATE and the caller is not authenticated.</summary>
        /// <param name="id">Workspace ID.</param>
        /// <response code="200">Workspace found.</response>
        /// <response code="403">Workspace is private and caller is not authenticated.</response>
        /// <response code="404">Workspace not found.</response>
        [HttpGet("{id}")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(FlowBoard.Workspace.Models.Workspace), 200)]
        [ProducesResponseType(403)]
        [ProducesResponseType(typeof(object), 404)]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var workspace = await _wsService.GetById(id);

                if (workspace.Visibility == "PRIVATE" && !User.Identity!.IsAuthenticated)
                    return Forbid();

                return Ok(workspace);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        /// <summary>Returns all workspaces owned by a specific user.</summary>
        /// <param name="ownerId">The user ID of the owner.</param>
        /// <response code="200">List of workspaces.</response>
        [HttpGet("owner/{ownerId}")]
        [Authorize]
        [ProducesResponseType(typeof(List<FlowBoard.Workspace.Models.Workspace>), 200)]
        public async Task<IActionResult> GetByOwner(string ownerId)
        {
            var workspaces = await _wsService.GetByOwner(ownerId);
            return Ok(workspaces);
        }

        /// <summary>Returns all workspaces the specified user is a member of.</summary>
        /// <param name="userId">The user ID to look up membership for.</param>
        /// <response code="200">List of workspaces.</response>
        [HttpGet("member/{userId}")]
        [Authorize]
        [ProducesResponseType(typeof(List<FlowBoard.Workspace.Models.Workspace>), 200)]
        public async Task<IActionResult> GetByMember(string userId)
        {
            var workspaces = await _wsService.GetByMember(userId);
            return Ok(workspaces);
        }

        /// <summary>Returns all PUBLIC workspaces. No authentication required.</summary>
        /// <response code="200">List of public workspaces.</response>
        [HttpGet("public")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(List<FlowBoard.Workspace.Models.Workspace>), 200)]
        public async Task<IActionResult> GetPublic()
        {
            var workspaces = await _wsService.GetPublicWorkspaces();
            return Ok(workspaces);
        }

        /// <summary>Updates workspace details.</summary>
        /// <param name="id">Workspace ID to update.</param>
        /// <param name="request">Updated workspace fields.</param>
        /// <response code="200">Updated workspace.</response>
        /// <response code="404">Workspace not found.</response>
        [HttpPut("{id}")]
        [Authorize]
        [ProducesResponseType(typeof(FlowBoard.Workspace.Models.Workspace), 200)]
        [ProducesResponseType(typeof(object), 404)]
        public async Task<IActionResult> Update(
            int id,
            [FromBody] UpdateWorkspaceRequest request)
        {
            try
            {
                var updated = await _wsService.UpdateWorkspace(id, request);
                return Ok(updated);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Deletes a workspace. Only the workspace owner can delete it.
        /// The owner ID is read from the JWT token.
        /// </summary>
        /// <param name="id">Workspace ID to delete.</param>
        /// <response code="204">Deleted successfully.</response>
        /// <response code="403">Caller is not the workspace owner.</response>
        /// <response code="404">Workspace not found.</response>
        [HttpDelete("{id}")]
        [Authorize]
        [ProducesResponseType(204)]
        [ProducesResponseType(403)]
        [ProducesResponseType(typeof(object), 404)]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var requestingUserId = GetCurrentUserId();
                await _wsService.DeleteWorkspace(id, requestingUserId);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
        }

        // ─────────────────────────────────────────────
        // MEMBER MANAGEMENT
        // ─────────────────────────────────────────────

        /// <summary>Adds a user as a member of the workspace with the given role (ADMIN or MEMBER).</summary>
        /// <param name="id">Workspace ID.</param>
        /// <param name="request">User ID and role to assign.</param>
        /// <response code="200">Member added successfully.</response>
        /// <response code="404">Workspace not found.</response>
        /// <response code="409">User is already a member of this workspace.</response>
        [HttpPost("{id}/members")]
        [Authorize]
        [ProducesResponseType(typeof(FlowBoard.Workspace.Models.WorkspaceMember), 200)]
        [ProducesResponseType(typeof(object), 404)]
        [ProducesResponseType(typeof(object), 409)]
        public async Task<IActionResult> AddMember(
            int id,
            [FromBody] AddMemberRequest request)
        {
            try
            {
                var member = await _wsService.AddMember(id, request);
                return Ok(member);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Removes a member from the workspace. The owner cannot be removed.
        /// The requesting user's ID is taken from the JWT token.
        /// </summary>
        /// <param name="id">Workspace ID.</param>
        /// <param name="userId">User ID of the member to remove.</param>
        /// <response code="204">Member removed successfully.</response>
        /// <response code="400">Cannot remove the workspace owner.</response>
        /// <response code="403">Caller is not an admin.</response>
        /// <response code="404">Workspace or member not found.</response>
        [HttpDelete("{id}/members/{userId}")]
        [Authorize]
        [ProducesResponseType(204)]
        [ProducesResponseType(typeof(object), 400)]
        [ProducesResponseType(403)]
        [ProducesResponseType(typeof(object), 404)]
        public async Task<IActionResult> RemoveMember(int id, string userId)
        {
            try
            {
                var requestingUserId = GetCurrentUserId();
                await _wsService.RemoveMember(id, userId, requestingUserId);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
        }

        /// <summary>Updates the role of an existing workspace member (ADMIN or MEMBER).</summary>
        /// <param name="id">Workspace ID.</param>
        /// <param name="userId">User ID of the member whose role is being updated.</param>
        /// <param name="newRole">New role: "ADMIN" or "MEMBER".</param>
        /// <response code="200">Updated member record.</response>
        /// <response code="404">Member not found in this workspace.</response>
        [HttpPut("{id}/members/{userId}/role")]
        [Authorize]
        [ProducesResponseType(typeof(FlowBoard.Workspace.Models.WorkspaceMember), 200)]
        [ProducesResponseType(typeof(object), 404)]
        public async Task<IActionResult> UpdateRole(
            int id,
            string userId,
            [FromBody] string newRole)
        {
            try
            {
                var member = await _wsService.UpdateMemberRole(id, userId, newRole);
                return Ok(member);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        /// <summary>Returns all members of a workspace.</summary>
        /// <param name="id">Workspace ID.</param>
        /// <response code="200">List of workspace members.</response>
        [HttpGet("{id}/members")]
        [Authorize]
        [ProducesResponseType(typeof(List<FlowBoard.Workspace.Models.WorkspaceMember>), 200)]
        public async Task<IActionResult> GetMembers(int id)
        {
            var members = await _wsService.GetMembers(id);
            return Ok(members);
        }

        [HttpGet("all")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> GetAllWorkspaces()
        {
            var workspaces = await _wsService.GetPublicWorkspaces(); 
            return Ok(workspaces);
        }
    }
}