namespace NotificationSystem.Interfaces
{
    interface INotificationService
    {
        void SendNotification(string message, int value); //method for sending a message

        void UnSendNotification(int id); //method for unsending the message

        void ShowAllMessages();//fetching all the message details
    }
}