using NotificationSystem.Models;

namespace NotificationSystem.Interfaces
{
    interface INotificationService
    {
        public Notification? SendNotification(string message, int value, int receiver_id); //method for creating a notification

        public Notification? UnsentNotification(int id); //method for deleting a notification

        public Notification? GetNotification(int id); //method for getting a particular notification 
        
        public List<Notification>? GetAllNotification(); //method for getting all the notification 
    }
}