namespace FlowBoard.Board.Exceptions
{
    public class BoardNotFoundException : Exception
    {
        public BoardNotFoundException(int boardId)
            : base($"Board {boardId} not found.") { }
    }

    public class BoardMemberNotFoundException : Exception
    {
        public BoardMemberNotFoundException(int boardId, int userId)
            : base($"Member {userId} not found on board {boardId}.") { }
    }

    public class BoardClosedException : Exception
    {
        public BoardClosedException(int boardId)
            : base($"Board {boardId} is closed and cannot be modified.") { }
    }

    public class DuplicateBoardMemberException : Exception
    {
        public DuplicateBoardMemberException(int userId)
            : base($"User {userId} is already a member of this board.") { }
    }

    public class InvalidBoardRoleException : Exception
    {
        public InvalidBoardRoleException(string role)
            : base($"Invalid role '{role}'. Must be one of: OBSERVER, MEMBER, ADMIN.") { }
    }
}