using NotificationModelLibrary;

namespace NotificationBLLibrary.Interfaces
{
    public interface INotificationSender
    {
        public void Send(User user, Notification notification); //method for sending notification

    }
}