using NotificationBLLibrary.Interfaces;
using NotificationModelLibrary;

namespace NotificationBLLibrary.Sender
{
    public class SmsNotificationSender : INotificationSender
    {
        //method for sending notification to the user
        public void Send(User user, Notification notification)
        {
            //setting the notification type as SmsNotification
            notification.NotificationType = NotType.SMSNotification;
            Console.WriteLine($"\nSMS SUCCESSFULLY SENDED TO {user.Name} VIA {user.PhoneNumber} AT {notification.SendedAt}"); ;
        }
    }
}