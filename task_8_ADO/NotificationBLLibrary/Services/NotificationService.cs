using NotificationDALLibrary.Repository;
using NotificationModelLibrary.Exceptions;
using NotificationModelLibrary;
using NotificationBLLibrary.Interfaces;
using NotificationDALLibrary.Interface;
using NotificationBLLibrary.Sender;

namespace NotificationBLLibrary.Services
{
    public class NotificationService : INotificationService
    {
        IUserService _user;

        //object for notification repo to perform curd operations
        IRepository<int, Notification> _notificationrepo = new NotificationRepository();

        //constructor for setting the user object used injection for over coming empty data issue
        public NotificationService(IUserService user)
        {
            _user = user;
        }

        //method for validating and creating a notification
        public Notification? SendNotification(INotificationSender sender, string message, int receiver_id)
        {
            //creating an object for notification
            Notification notification = new Notification(message, DateTime.Now, true, receiver_id);

            var result = _user.GetUserDetails(receiver_id); //getting user details

            //check it the result is empty
            if (result == null)
            {
                return null;
            }

            //method for validating the message
            if (!ValidateMessage(notification, sender, result))
            {
                return null;
            }

            sender.Send(result, notification);

            return _notificationrepo.Create(notification);
        }

        //method for deleting the notification 
        public Notification? UnsentNotification(int id)
        {
            //check if the id is valid
            if (id > 0)
            {
                return _notificationrepo.Delete(id);
            }
            return null;
        }

        //method for getting a particular notification
        public Notification? GetNotification(int id)
        {
            //check if the id is valid
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

        //method for sending notification to all users
        public List<Notification>? SendAllUsers(INotificationSender sender, string message)
        {
            //get all the user details
            var users = _user.GetAllUserDetails();

            //check the user dict is not empty
            if (users != null)
            {
                //loop through each user
                foreach (var u in users)
                {
                    //used try catch block to handling exception while creating a single user
                    try
                    {
                        //create new notification object for each user
                        Notification notification1 = new Notification(message, DateTime.Now, true, u.UserId);
                        if (ValidateMessage(notification1, sender, u))
                        {
                            sender.Send(u, notification1);
                            _notificationrepo.Create(notification1); //creating notification
                        }
                    }
                    catch (InvalidInputExceptions e)
                    {
                        Console.WriteLine($"SKIPPED {u.Name} : {e.Message}");
                    }
                }
                return _notificationrepo.GetAllData();
            }
            return null;
        }

        public bool ValidateMessage(Notification notification, INotificationSender sender, User result)
        {
            //check message is not empty
            if (string.IsNullOrWhiteSpace(notification.Message))
            {
                throw new InvalidInputExceptions("PLEASE ENTER MESSAGE! MESSAGE SHOULD NOT BE EMPTY");
            }
            //check the email is valid 
            else if (sender is EmailNotificationSender && !result.Email.Contains("@"))
            {
                throw new InvalidInputExceptions("INVALID EMAIL FOR EMAIL NOTIFICATION");
            }
            //check the phone number is valid
            else if (sender is SmsNotificationSender && result.PhoneNumber.Length != 10)
            {
                throw new InvalidInputExceptions("INVALID PHONE FOR SMS NOTIFICATION");
            }
            //check message has length more than 5
            else if (notification.Message.Length < 5)
            {
                throw new InvalidInputExceptions("PLEASE ENTER A MESSAGE WITH MORE THAN 5 CHARACTERS");
            }
            //check message has length less than 160
            else if (notification.Message.Length > 160)
            {
                throw new InvalidInputExceptions("PLEASE ENTER A MESSAGE WITH LESS THAN 160 CHARACTERS");
            }

            return true;
        }
    }
}