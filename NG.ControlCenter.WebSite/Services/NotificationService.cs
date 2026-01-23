namespace NG.ControlCenter.WebSite.Services
{
    /// <summary>
    /// Centralized notification service for consistent error and success messages
    /// </summary>
    public interface INotificationService
    {
        void ShowSuccess(string message);
        void ShowError(string message, string? details = null);
        void ShowWarning(string message);
        void ShowInfo(string message);
    }

    public class NotificationMessage
    {
        public NotificationType Type { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? Details { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public string Id { get; set; } = Guid.NewGuid().ToString();
    }

    public enum NotificationType
    {
        Success,
        Error,
        Warning,
        Info
    }

    public class NotificationService : INotificationService
    {
        private List<NotificationMessage> _notifications = new();
        public event Action<NotificationMessage>? OnNotification;

        public void ShowSuccess(string message)
        {
            AddNotification(NotificationType.Success, message);
        }

        public void ShowError(string message, string? details = null)
        {
            AddNotification(NotificationType.Error, message, details);
        }

        public void ShowWarning(string message)
        {
            AddNotification(NotificationType.Warning, message);
        }

        public void ShowInfo(string message)
        {
            AddNotification(NotificationType.Info, message);
        }

        private void AddNotification(NotificationType type, string message, string? details = null)
        {
            var notification = new NotificationMessage
            {
                Type = type,
                Message = message,
                Details = details
            };

            _notifications.Add(notification);
            OnNotification?.Invoke(notification);

            // Auto-remove after 5 seconds
            Task.Delay(5000).ContinueWith(_ => _notifications.Remove(notification));
        }
    }
}
