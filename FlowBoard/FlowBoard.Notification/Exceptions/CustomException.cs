namespace FlowBoard.Notification.Exceptions
{
    public class NotificationNotFoundException : Exception
    {
        public NotificationNotFoundException(int notificationId)
            : base($"Notification {notificationId} not found.") { }
    }

    public class EmailSendException : Exception
    {
        public EmailSendException(string toEmail, string status)
            : base($"Failed to send email to {toEmail}. Status: {status}.") { }
    }

    public class UnauthorizedNotificationAccessException : Exception
    {
        public UnauthorizedNotificationAccessException()
            : base("You are not allowed to access this notification.") { }
    }
}