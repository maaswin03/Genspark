namespace NotificationSystem.Models
{

    public enum NotType
    {
        EmailNotification = 1,
        SMSNotification = 2
    }

    class Notification
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
        public int ReceiverId { get; set; }

        public Notification()
        {

        }

//constructor to set value for message
        public Notification(string message)
        {
            Message = message;
        }

        public override string ToString()
        {
            return $"{MessageId} - {Message} - {SendedAt} - {NotificationType}";
        }
    }
}