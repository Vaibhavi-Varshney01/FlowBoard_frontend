using FlowBoard.List.Models;
using FlowBoard.List.Repositories;

namespace FlowBoard.List.Services
{
    public class ListServiceImpl : IListService
    {
        private readonly IListRepository _listRepository;

        public ListServiceImpl(IListRepository listRepository)
        {
            _listRepository = listRepository;
        }

        // ── CRUD ──────────────────────────────────────────────────────────────

        public async Task<TaskList> CreateList(CreateListRequest request)
        {
            var maxPosition = await _listRepository.FindMaxPositionByBoardId(request.BoardId);

            var list = new TaskList
            {
                BoardId    = request.BoardId,
                Name       = request.Name,
                Color      = request.Color,
                Position   = maxPosition + 1,
                IsArchived = false,
                CreatedAt  = DateTime.UtcNow,
                UpdatedAt  = DateTime.UtcNow
            };

            return await _listRepository.Save(list);
        }

        public async Task<TaskList> GetListById(int listId)
            => await _listRepository.FindByListId(listId)
               ?? throw new KeyNotFoundException($"List {listId} not found.");

        public async Task<List<TaskList>> GetListsByBoard(int boardId)
            => await _listRepository.FindByBoardIdOrderByPosition(boardId);

        public async Task<TaskList> UpdateList(int listId, UpdateListRequest request)
        {
            var list = await _listRepository.FindByListId(listId)
                ?? throw new KeyNotFoundException($"List {listId} not found.");

            if (list.IsArchived)
                throw new InvalidOperationException($"List {listId} is archived and cannot be updated.");

            list.Name      = request.Name;
            list.Color     = request.Color;
            list.UpdatedAt = DateTime.UtcNow;

            return await _listRepository.Update(list);
        }

        public async Task DeleteList(int listId)
        {
            var list = await _listRepository.FindByListId(listId)
                ?? throw new KeyNotFoundException($"List {listId} not found.");

            var siblings = await _listRepository.FindByBoardIdOrderByPosition(list.BoardId);

            await _listRepository.DeleteByListId(listId);

            var remaining = siblings
                .Where(l => l.ListId != listId)
                .OrderBy(l => l.Position)
                .ToList();

            for (int i = 0; i < remaining.Count; i++)
                remaining[i].Position = i + 1;

            if (remaining.Any())
                await _listRepository.UpdateRangePositions(remaining);
        }

        // ── Position management ───────────────────────────────────────────────

        public async Task<List<TaskList>> ReorderLists(int boardId, ReorderListsRequest request)
        {
            var activeLists = await _listRepository.FindByBoardIdOrderByPosition(boardId);

            if (request.Positions.Count != activeLists.Count)
                throw new ArgumentException(
                    $"Position payload has {request.Positions.Count} entries but board has {activeLists.Count} active lists.");

            var listMap = activeLists.ToDictionary(l => l.ListId);

            foreach (var entry in request.Positions)
            {
                if (!listMap.ContainsKey(entry.ListId))
                    throw new ArgumentException(
                        $"List {entry.ListId} does not belong to board {boardId} or is archived.");
            }

            foreach (var entry in request.Positions)
                listMap[entry.ListId].Position = entry.Position;

            await _listRepository.UpdateRangePositions(activeLists);

            return await _listRepository.FindByBoardIdOrderByPosition(boardId);
        }

        // ── Archival ─────────────────────────────────────────────────────────

        public async Task<TaskList> ArchiveList(int listId)
        {
            var list = await _listRepository.FindByListId(listId)
                ?? throw new KeyNotFoundException($"List {listId} not found.");

            if (list.IsArchived)
                throw new InvalidOperationException($"List {listId} is already archived.");

            list.IsArchived = true;
            list.UpdatedAt  = DateTime.UtcNow;

            var updated = await _listRepository.Update(list);

            await CompactPositions(list.BoardId);

            return updated;
        }

        public async Task<TaskList> UnarchiveList(int listId)
        {
            var list = await _listRepository.FindByListId(listId)
                ?? throw new KeyNotFoundException($"List {listId} not found.");

            if (!list.IsArchived)
                throw new InvalidOperationException($"List {listId} is not archived.");

            var maxPosition = await _listRepository.FindMaxPositionByBoardId(list.BoardId);

            list.IsArchived = false;
            list.Position   = maxPosition + 1;
            list.UpdatedAt  = DateTime.UtcNow;

            return await _listRepository.Update(list);
        }

        public async Task<List<TaskList>> GetArchivedLists(int boardId)
            => await _listRepository.FindByBoardIdAndIsArchived(boardId, isArchived: true);

        // ── Board transfer ────────────────────────────────────────────────────

        public async Task<TaskList> MoveList(int listId, MoveListRequest request)
        {
            var list = await _listRepository.FindByListId(listId)
                ?? throw new KeyNotFoundException($"List {listId} not found.");

            if (list.BoardId == request.TargetBoardId)
                throw new InvalidOperationException(
                    $"List {listId} is already on board {request.TargetBoardId}.");

            var originBoardId = list.BoardId;

            var targetMaxPosition = await _listRepository.FindMaxPositionByBoardId(request.TargetBoardId);

            list.BoardId   = request.TargetBoardId;
            list.Position  = targetMaxPosition + 1;
            list.UpdatedAt = DateTime.UtcNow;

            var moved = await _listRepository.Update(list);

            await CompactPositions(originBoardId);

            return moved;
        }

        // ── SAGA ─────────────────────────────────────────────────────────────

        // Board Service call karta hai jab board delete hota hai
        public async Task DeleteAllListsByBoard(int boardId)
        {
            var lists = await _listRepository.FindByBoardId(boardId);

            foreach (var list in lists)
                await _listRepository.DeleteByListId(list.ListId);

            Console.WriteLine(
                $"[SAGA] Board {boardId} ki {lists.Count} lists delete hui.");
        }

        // ── Helpers ───────────────────────────────────────────────────────────

        private async Task CompactPositions(int boardId)
        {
            var activeLists = await _listRepository.FindByBoardIdOrderByPosition(boardId);
            for (int i = 0; i < activeLists.Count; i++)
                activeLists[i].Position = i + 1;

            if (activeLists.Any())
                await _listRepository.UpdateRangePositions(activeLists);
        }
    }
}