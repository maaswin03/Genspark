using NotificationSystem.Models;
using NotificationSystem.Interfaces;
using NotificationSystem.Repository;

namespace NotificationSystem.Services
{
    class NotificationService : INotificationService
    {
        UserService user = new UserService();

        //object for notification repo to perform curd operations
        NotificationRepository _notificationrepo = new NotificationRepository();

        //method for validating and creating a notification
        public Notification? SendNotification(string message, int value, int receiver_id)
        {
            Notification? notification = null;

            if (value == 1) //check the value is 1 for email
            {
                notification = new EmailNotification();
                var result = user.GetUserDetails(receiver_id); //finding user id
                notification.ReceiverId = (result != null) ? result.UserId : 0;
            }
            else if (value == 2) //check the value is 2 for sms
            {
                notification = new SMSNotification();
                var result = user.GetUserDetails(receiver_id); // finding user id
                notification.ReceiverId = (result != null) ? result.UserId : 0;
            }

            if (notification == null)
            {
                return null;
            }

            notification.SendedAt = DateTime.Now;
            notification.Message = message;
            notification.NotificationSent = true;

            return _notificationrepo.Create(notification);
        }

        //method for deleting the notification 
        public Notification? UnsentNotification(int id)
        {
            if (id > 0)
            {
                return _notificationrepo.Delete(id);
            }
            return null;
        }

        //method for getting a particular notification
        public Notification? GetNotification(int id)
        {
            if (id > 0)
            {
                return _notificationrepo.GetData(id);
            }
            return null;
        }

        //method for getting all the notification data
        public List<Notification>? GetAllNotification()
        {
            return _notificationrepo.GetAllData();
        }
    }
}