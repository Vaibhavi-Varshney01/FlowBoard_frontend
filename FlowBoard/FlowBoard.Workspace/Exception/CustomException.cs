namespace FlowBoard.Workspace.Exceptions
{
    public class WorkspaceNotFoundException : Exception
    {
        public WorkspaceNotFoundException(string message = "Workspace not found") : base(message) { }
    }

    public class WorkspaceAlreadyExistsException : Exception
    {
        public WorkspaceAlreadyExistsException(string message = "A workspace with this name already exists for this user") : base(message) { }
    }

    public class MemberAlreadyExistsException : Exception
    {
        public MemberAlreadyExistsException(string message = "User is already a member of this workspace") : base(message) { }
    }

    public class MemberNotFoundException : Exception
    {
        public MemberNotFoundException(string message = "Member not found in this workspace") : base(message) { }
    }

    public class CannotRemoveOwnerException : Exception
    {
        public CannotRemoveOwnerException(string message = "The workspace owner cannot be removed") : base(message) { }
    }

    public class UnauthorizedWorkspaceAccessException : Exception
    {
        public UnauthorizedWorkspaceAccessException(string message = "You do not have permission to perform this action") : base(message) { }
    }
}