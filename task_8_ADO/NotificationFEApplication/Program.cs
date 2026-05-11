using NotificationModelLibrary.Exceptions;
using NotificationBLLibrary.Services;
using NotificationModelLibrary;
using NotificationBLLibrary.Interfaces;
using NotificationBLLibrary.Sender;
using System.Linq.Expressions;

namespace NotificationFEApplication
{
    public class Program
    {

        static IUserService user = new UserService();

        static INotificationService notification = new NotificationService(user);

        //method for calling create user and getting user inputs
        public static void CreateUser()
        {
            try
            {
                Console.WriteLine("\nPLEASE ENTER NAME : ");
                string Name = Console.ReadLine() ?? ""; //getting name as an input

                Console.WriteLine("\nPLEASE ENTER EMAIL : ");
                string Email = Console.ReadLine() ?? ""; //getting email as an input

                Console.WriteLine("\nPLEASE ENTER PHONE NUMBER : ");
                string Phone = Console.ReadLine() ?? ""; //getting phone number as an input 

                DateTime CurrentDate = DateTime.Now; //storing current date and time 

                var result = user.CreateUser(Name.ToUpper(), Email, Phone, CurrentDate); //calling the method 

                if (result != null)
                {
                    Console.WriteLine("\nUSER CREATED SUCCESSFULLY !");
                }
                else
                {
                    Console.WriteLine("\nUSER CREATION FAILED! PLEASE TRY AGAIN LATER");
                }
            }
            catch (InvalidInputExceptions e)
            {
                Console.WriteLine($"\n{e.Message}");
            }
        }

        //method for calling delete method and showing result
        public static void DeleteUser()
        {
            Console.WriteLine("\nPLEASE ENTER USER ID FOR DELETION : ");

            int id = 0;
            while (!int.TryParse(Console.ReadLine(), out id) || (id < 0)) //getting user id as input 
            {
                Console.WriteLine("\nPLEASE ENTER A VALID USER ID");
            }

            var result = user.DeleteUser(id); //calling delete method for deletion 

            if (result != null)
            {
                Console.WriteLine("\nUSER DELETED SUCCESSFULLY !");
            }
            else
            {
                Console.WriteLine("\nNO USER WITH THE USER ID EXISTS IN THE DATABASE");
            }
        }

        //method for calling method for fetching user details based on user id
        public static void FetchUser()
        {
            Console.WriteLine("\nPLEASE ENTER USER ID FOR FETCHING : ");

            int id = 0;
            while (!int.TryParse(Console.ReadLine(), out id) || (id < 0)) //getting user id from user
            {
                Console.WriteLine("\nPLEASE ENTER A VALID USER ID");
            }

            var result = user.GetUserDetails(id); //calling the method

            if (result != null)
            {
                Console.WriteLine("\nBELOW ARE THE USER DETAILS");
                //showing result
                Console.WriteLine($"\n{result.UserId} -  {result.Name} -  {result.Email} - +91-{result.PhoneNumber}");
            }
            else
            {
                Console.WriteLine("\nNO USER WITH THE MENTIONED USER ID EXISTS IN THE DATABASE");
            }
        }

        //method for calling user service method for fetching all users 
        public static void FetchAllUser()
        {
            var results = user.GetAllUserDetails();

            if (results != null)
            {
                Console.WriteLine("\nBELOW ARE THE USERS DETAILS");
                //showing all the user details
                foreach (var result in results)
                {
                    Console.WriteLine($"\n{result.UserId} -  {result.Name} -  {result.Email} - +91-{result.PhoneNumber}");
                }
            }
            else
            {
                Console.WriteLine("\nNO USER PRESENT IN THE DATABASE . PLEASE CREATE A NEW USER !");
            }
        }


        //method for calling the update method and getting user inputs
        public static void UpdateUser()
        {
            try
            {
                Console.WriteLine("\nPLEASE ENTER USER ID FOR UPDATING : ");
                int id = 0;
                while (!int.TryParse(Console.ReadLine(), out id) || (id < 0)) //getting user id 
                {
                    Console.WriteLine("\nPLEASE ENTER A VALID USER ID");
                }

                Console.WriteLine("\nPLEASE ENTER THE FIELD FOR UPDATE");
                Console.WriteLine("ENTER 1 FOR UPDATING NAME");
                Console.WriteLine("ENTER 2 FOR UPDATING EMAIL");
                Console.WriteLine("ENTER 3 FOR UPDATING PHONE NUMBER");
                int field = 0;
                while (!int.TryParse(Console.ReadLine(), out field) || (field != 1 && field != 2 && field != 3))
                {
                    Console.WriteLine("\nPLEASE ENTER A VALID NUMBER");
                }

                string field_value = "";

                if (field == 1)
                {
                    Console.WriteLine("\nPLEASE ENTER NAME : ");
                    field_value = Console.ReadLine() ?? ""; // getting name 
                }
                else if (field == 2)
                {
                    Console.WriteLine("\nPLEASE ENTER EMAIL : ");
                    field_value = Console.ReadLine() ?? ""; //getting email
                }
                else if (field == 3)
                {
                    Console.WriteLine("\nPLEASE ENTER PHONE NUMBER : ");
                    field_value = Console.ReadLine() ?? ""; // getting phone 
                }


                var result = user.UpdateUserDetails(id, field , field_value);

                if (result != null)
                {
                    //showing status
                    Console.WriteLine("\nUSER UPDATED SUCCESSFULLY !");
                }
                else
                {
                    Console.WriteLine("\nUSER CREATION FAILED! PLEASE TRY AGAIN LATER");
                }
            }
            catch (InvalidInputExceptions e)
            {
                Console.WriteLine($"\n{e.Message}");
            }
        }

        //method for calling create notification and getting input values
        public static void SendNotification()
        {
            try
            {
                Console.WriteLine("\nPLEASE MESSAGE FOR SENDING : ");
                string Message = Console.ReadLine() ?? ""; // getting message for sending 

                Console.WriteLine("\nBELOW ARE THE AVAILABLE MODES FOR COMMUNICATION");
                Console.WriteLine("ENTER 1 FOR EMAIL NOTIFICATION ");
                Console.WriteLine("ENTER 2 FOR SMS NOTIFICATION ");
                Console.WriteLine("\nPLEASE ENTER THE VALUE : ");
                int value = 0;
                while (!int.TryParse(Console.ReadLine(), out value) || (value != 1 && value != 2)) //getting notification type 
                {
                    Console.WriteLine("\nPLEASE ENTER A VALID VALUE");
                }

                Console.WriteLine("\nPLEASE ENTER USER ID FOR SENDING: ");
                int id = 0;
                while (!int.TryParse(Console.ReadLine(), out id) || (id < 0)) //getting user id 
                {
                    Console.WriteLine("\nPLEASE ENTER A VALID USER ID");
                }

                INotificationSender sender = (value == 1) ? new EmailNotificationSender() : new SmsNotificationSender();

                var result = notification.SendNotification(sender, Message, id);

                if (result == null)
                {
                    Console.WriteLine($"\nSENDING FAILED! PLEASE TRY AGAIN LATER");
                }
            }
            catch (InvalidInputExceptions e)
            {
                Console.WriteLine(e.Message);
            }
        }

        //method for getting user id and calling the unsent notification method
        public static void UnsentNotification()
        {
            try
            {
                Console.WriteLine("\nPLEASE ENTER MESSAGE ID FOR UNSENDING : ");
                int id = 0;
                while (!int.TryParse(Console.ReadLine(), out id) || (id < 0)) //getting message id as input 
                {
                    Console.WriteLine("\nPLEASE ENTER A VALID MESSAGE ID");
                }

                var result = notification.UnsentNotification(id); //calling delete method for deletion 

                if (result != null)
                {
                    Console.WriteLine("\nMESSAGE UN SENDED SUCCESSFULLY !");
                }
                else
                {
                    Console.WriteLine("\nNO MESSAGE WITH THE MESSAGE ID EXISTS IN THE DATABASE");
                }
            }
            catch (InvalidInputExceptions e)
            {
                Console.WriteLine($"\n{e.Message}");
            }
        }

        //method for sending notification to all
        public static void SendNotificationToAll()
        {
            try
            {
                Console.WriteLine("\nPLEASE ENTER THE MESSAGE");
                string message = Console.ReadLine() ?? "";

                Console.WriteLine("\nPLEASE SELECT A MODE OF COMMUNICATION");
                Console.WriteLine("ENTER 1 FOR EMAIL NOTIFICATION");
                Console.WriteLine("ENTER 2 FOR SMS NOTIFICATION");

                Console.WriteLine("\nPLEASE ENTER THE NUMBER FOR MODE : ");
                int value = 0;
                while (!int.TryParse(Console.ReadLine(), out value) || (value != 1 && value != 2))
                {
                    Console.WriteLine("\nPLEASE ENTER A VALID NUMBER");
                }

                INotificationSender sender = (value == 1) ? new EmailNotificationSender() : new SmsNotificationSender();

                var result = notification.SendAllUsers(sender, message);

                if (result == null)
                {
                    Console.WriteLine("NOTIFICATION FAILED TO SEND TO THE USERS");
                }
            }
            catch (InvalidInputExceptions e)
            {
                Console.WriteLine($"\n{e.Message}");
            }

        }

        //method for fetching all notification
        public static void GetAllNotification()
        {
            var results = notification.GetAllNotification();

            if (results != null && results.Count > 0)
            {
                //showing all the user details
                foreach (var result in results)
                {
                    string value = result.NotificationSent ? "SENDED" : "UNSENT";
                    Console.WriteLine($"\n{result.MessageId} -  {result.Message} -  {result.SendedAt} - {result.NotificationType} - TO : {result.UserName} - {value}");
                }
            }
            else
            {
                Console.WriteLine("\nNO MESSAGES EXISTS IN THE DATABASE");
            }
        }

        //main method
        public static void Main(String[] args)
        {

            while (true)
            {
                //available options in the notification system
                Console.WriteLine("------------------------------------------");
                Console.WriteLine();
                Console.WriteLine("ENTER 1 FOR CREATING A NEW USER");
                Console.WriteLine("ENTER 2 FOR DELETING USER");
                Console.WriteLine("ENTER 3 FOR FETCHING A USER");
                Console.WriteLine("ENTER 4 FOR FETCHING ALL USER");
                Console.WriteLine("ENTER 5 FOR UPDATING A USER");
                Console.WriteLine("ENTER 6 FOR SENDING A NOTIFICATION");
                Console.WriteLine("ENTER 7 FOR UNSENDING A NOTIFICATION");
                Console.WriteLine("ENTER 8 FOR SENDING NOTIFICATION TO ALL");
                Console.WriteLine("ENTER 9 FOR FETCHING ALL NOTIFICATION");
                Console.WriteLine("ENTER 0 FOR EXIT");
                Console.WriteLine();
                Console.WriteLine("------------------------------------------");
                Console.WriteLine("\nPLEASE ENTER THE VALUE : ");

                int n = 0;
                while (!int.TryParse(Console.ReadLine(), out n) || (n < 0 || n > 9))
                {
                    Console.WriteLine("PLEASE ENTER A VALID NUMBER\n");
                }

                if (n == 1) //create user
                {
                    CreateUser();
                }
                else if (n == 2) //delete user
                {
                    DeleteUser();
                }
                else if (n == 3) //fetch user by id
                {
                    FetchUser();
                }
                else if (n == 4) //fetch all user
                {
                    FetchAllUser();
                }
                else if (n == 5) //update user by id
                {
                    UpdateUser();
                }
                else if (n == 6) //sending notification
                {
                    SendNotification();
                }
                else if (n == 7) //unsending notification
                {
                    UnsentNotification();
                }
                else if (n == 8) // fetching message by id
                {
                    SendNotificationToAll();
                }
                else if (n == 9) //fetch all notification
                {
                    GetAllNotification();
                }
                else if (n == 0) // exit console
                {
                    break;
                }
            }
        }
    }
}