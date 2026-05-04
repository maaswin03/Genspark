using NotificationSystem.Interfaces;

namespace NotificationSystem.Models
{
    class SMSNotification : Notification, INotification //inheriting from parent class notification
    {
        public SMSNotification()
        {
            NotificationType = NotType.SMSNotification; //updating the notification type as sms
        }
        public void Send()
        {
            Console.WriteLine("SMS NOTIFICATION SENT SUCCESSFULLY");
        }
    }
}