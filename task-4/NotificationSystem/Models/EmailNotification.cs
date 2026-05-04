using NotificationSystem.Interfaces;

namespace NotificationSystem.Models
{
    class EmailNotification : Notification , INotification //inheriting from notification call
    {
        public EmailNotification()
        {
            NotificationType = NotType.EmailNotification; //updating the type as email notification
        }

        public void Send()
        {
            Console.WriteLine("EMAIL NOTIFICATION SENT SUCCESSFULLY");
        }
    }
}