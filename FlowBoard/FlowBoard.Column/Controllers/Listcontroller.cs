using FlowBoard.List.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FlowBoard.List.Controllers
{
    [ApiController]
    [Route("api/lists")]
    [Authorize]
    public class ListController : ControllerBase
    {
        private readonly IListService _listService;

        public ListController(IListService listService)
        {
            _listService = listService;
        }

        // POST /api/lists
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateListRequest request)
        {
            try
            {
                var list = await _listService.CreateList(request);
                return Ok(list);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // GET /api/lists/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var list = await _listService.GetListById(id);
                return Ok(list);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        // GET /api/lists/board/{boardId}
        [HttpGet("board/{boardId}")]
        public async Task<IActionResult> GetByBoard(int boardId)
        {
            var lists = await _listService.GetListsByBoard(boardId);
            return Ok(lists);
        }

        // GET /api/lists/board/{boardId}/archived
        [HttpGet("board/{boardId}/archived")]
        public async Task<IActionResult> GetArchived(int boardId)
        {
            var lists = await _listService.GetArchivedLists(boardId);
            return Ok(lists);
        }

        // PUT /api/lists/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateListRequest request)
        {
            try
            {
                var list = await _listService.UpdateList(id, request);
                return Ok(list);
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

        // PUT /api/lists/board/{boardId}/reorder
        [HttpPut("board/{boardId}/reorder")]
        public async Task<IActionResult> Reorder(int boardId, [FromBody] ReorderListsRequest request)
        {
            try
            {
                var lists = await _listService.ReorderLists(boardId, request);
                return Ok(lists);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        // PUT /api/lists/{id}/move
        [HttpPut("{id}/move")]
        public async Task<IActionResult> Move(int id, [FromBody] MoveListRequest request)
        {
            try
            {
                var list = await _listService.MoveList(id, request);
                return Ok(list);
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

        // POST /api/lists/{id}/archive
        [HttpPost("{id}/archive")]
        public async Task<IActionResult> Archive(int id)
        {
            try
            {
                var list = await _listService.ArchiveList(id);
                return Ok(list);
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

        // POST /api/lists/{id}/unarchive
        [HttpPost("{id}/unarchive")]
        public async Task<IActionResult> Unarchive(int id)
        {
            try
            {
                var list = await _listService.UnarchiveList(id);
                return Ok(list);
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

        // DELETE /api/lists/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _listService.DeleteList(id);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        // ADDED: SAGA endpoint — Board Service call karta hai jab board delete hota hai
        // DELETE /api/lists/board/{boardId}/all
        [HttpDelete("board/{boardId}/all")]
        public async Task<IActionResult> DeleteAllByBoard(int boardId)
        {
            await _listService.DeleteAllListsByBoard(boardId);
            return NoContent();
        }
    }
}