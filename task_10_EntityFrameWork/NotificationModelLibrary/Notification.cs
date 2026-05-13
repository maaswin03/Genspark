using System.ComponentModel.DataAnnotations;

namespace NotificationModelLibrary
{

    public enum NotType
    {
        EmailNotification = 1,
        SMSNotification = 2
    }

    public class Notification
    {
        //variable for storing message id
        public int MessageId { get; set; }

        //variable for storing message
        public string Message { get; set; } = string.Empty;

        //variable for storing message sended time
        public DateTime SendedAt { get; set; }

        //variable for storing notification type
        public NotType NotificationType { get; set; }

        //variable for storing the notification sent status
        public bool NotificationSent { get; set; }

        //variable for storing receiver userId
        public int? ReceiverId { get; set; }

        public string UserName { get; set; } = string.Empty;

        public User? User { get; set; } = null;

        public Notification()
        {

        }

        //constructor to set value for message
        public Notification(string message, DateTime sendat, bool status, int user_id)
        {
            Message = message;
            SendedAt = sendat;
            NotificationSent = status;
            ReceiverId = user_id;
        }

        //method for printing notification
        public override string ToString()
        {
            return $"{Message} - {SendedAt} - {NotificationSent} - {ReceiverId}";
        }

    }
}