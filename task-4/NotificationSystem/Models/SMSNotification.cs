namespace NotificationSystem.Models 
{
    class SMSNotification : Notification //inheriting from parent class notification
    {
        public SMSNotification()
        {
            NotificationType = NotType.SMSNotification; //updating the notification type as sms
        }
    }
}