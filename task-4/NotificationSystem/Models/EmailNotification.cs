namespace NotificationSystem.Models
{
    class EmailNotification : Notification //inheriting from notification call
    {
        public EmailNotification()
        {
            NotificationType = NotType.EmailNotification; //updating the type as email notification
        }
    }
} 