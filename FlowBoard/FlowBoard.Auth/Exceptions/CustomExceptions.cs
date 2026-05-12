namespace FlowBoard.Auth.Exceptions
{
    public class UserNotFoundException : Exception
    {
        public UserNotFoundException(string message = "User not found") : base(message) { }
    }

    public class InvalidCredentialsException : Exception
    {
        public InvalidCredentialsException(string message = "Invalid credentials") : base(message) { }
    }

    public class EmailAlreadyExistsException : Exception
    {
        public EmailAlreadyExistsException(string message = "Email already exists") : base(message) { }
    }

    public class UsernameAlreadyExistsException : Exception
    {
        public UsernameAlreadyExistsException(string message = "Username already exists") : base(message) { }
    }

    public class AccountDeactivatedException : Exception
    {
        public AccountDeactivatedException(string message = "Account is deactivated") : base(message) { }
    }

    public class InvalidTokenException : Exception
    {
        public InvalidTokenException(string message = "Invalid or expired token") : base(message) { }
    }
}