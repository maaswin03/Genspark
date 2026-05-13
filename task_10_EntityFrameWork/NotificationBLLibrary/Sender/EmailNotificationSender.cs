using NotificationBLLibrary.Interfaces;
using NotificationModelLibrary;

namespace NotificationBLLibrary.Sender
{
    public class EmailNotificationSender : INotificationSender
    {
        //method to send email notification
        public void Send(User user , Notification notification)
        {
            //setting the notification type as email;
            notification.NotificationType = NotType.EmailNotification;
            Console.WriteLine($"\nEMAIL SUCCESSFULLY SENDED TO {user.Name} AT {notification.SendedAt}");
        }
    }
}