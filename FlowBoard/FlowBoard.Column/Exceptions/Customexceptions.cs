namespace FlowBoard.List.Exceptions
{
    public class ListNotFoundException : Exception
    {
        public ListNotFoundException(int listId)
            : base($"List {listId} not found.") { }
    }

    public class ListArchivedException : Exception
    {
        public ListArchivedException(int listId)
            : base($"List {listId} is archived and cannot be modified.") { }
    }

    public class ListNotArchivedException : Exception
    {
        public ListNotArchivedException(int listId)
            : base($"List {listId} is not archived.") { }
    }

    public class InvalidListPositionException : Exception
    {
        public InvalidListPositionException(string message)
            : base(message) { }
    }

    public class ListAlreadyOnBoardException : Exception
    {
        public ListAlreadyOnBoardException(int listId, int boardId)
            : base($"List {listId} is already on board {boardId}.") { }
    }
}