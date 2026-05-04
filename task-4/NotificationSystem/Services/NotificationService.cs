using NotificationSystem.Models;
using NotificationSystem.Interfaces;

namespace NotificationSystem.Services
{
    class NotificationService : INotificationService
    {
        List<Notification> NotificationStack = new List<Notification>(); //object for notification class

        public int LastMessageid = 0;

        private IUserService User;

        public NotificationService(IUserService user)
        {
            User = user;
        }

        //method for sending a new message
        public void SendNotification(string message, int value)
        {
            Notification? notification = null;

            if (value == 1)
            {
                notification = new EmailNotification();
                Console.WriteLine("ENTER THE RECEIVER EMAIL ID : "); //asking email for Email notification
                string Email = Console.ReadLine() ?? "";
                int id = User.GetUserDetailsByEmail(Email); // getting user id
                notification.ReceiverId = id;

            }
            else if (value == 2)
            {
                notification = new SMSNotification();
                Console.WriteLine("ENTER THE RECEIVER PHONE NUMBER : "); //asking phone for sms
                string PhoneNumber = Console.ReadLine() ?? "";
                int id = User.GetUserDetailsByPhone(PhoneNumber); // getting user id
                notification.ReceiverId = id;
            }

            if (notification == null)
            {
                Console.WriteLine("INVALID NOTIFICATION TYPE SELECTED");
                return;
            }

            notification.Message = message;
            LastMessageid++;
            notification.MessageId = LastMessageid; //creating a unique user id
            notification.NotificationSent = true;//setting status as true
            notification.SendedAt = DateTime.Now;

            if (notification.ReceiverId > 0) // validating user id not equal to 0
            {
                NotificationStack.Add(notification); // adding to the list
                ((INotification)notification).Send();
            }
            else
            {
                Console.WriteLine("NOTIFICATION FAILED DUE TO TECHNICAL ISSUE");
            }
        }

        //method for unsending the message based on message id
        public void UnSendNotification(int id)
        {
            var item = NotificationStack.FirstOrDefault(x => x.MessageId == id); //fetching message id
            if (item != null)
            {
                item.NotificationSent = false;//making status as false
                Console.Write("MESSAGE UNSENEDED SUCCESSFULLY");
            }
            else
            {
                Console.Write("NO MESSAGE WITH THE MESSAGE ID IS PRESENT");
            }
        }

        //method for showing all messages
        public void ShowAllMessages()
        {
            if (NotificationStack.Count == 0) //checking the list is not empty
            {
                Console.WriteLine("NO MESSAGE SENT CURRENTLY . PLEASE SEND A NEW MESSAGE");
            }
            else
            {
                Console.WriteLine("BELOW ARE THE MESSAGE SENT RECENTLY");
                foreach (var item in NotificationStack)
                {
                    PrintDetails(item); //sending each message for printing
                }
            }
        }

        public void PrintDetails(Notification notification)
        {
            Console.WriteLine("-----------------------------");
            Console.WriteLine(notification);
            Console.WriteLine("-----------------------------");
        }
    }
}