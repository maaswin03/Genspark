using NotificationSystem.Models;
using NotificationSystem.Interfaces;
using NotificationSystem.Services;
using System.Reflection.Metadata;

namespace NotificationSystem
{
    class Program
    {

        public static void Main(String[] args)
        {
            IUserService User = new UserService(); // object fot user service

            INotificationService Notification = new NotificationService(User); // object for notification service

            //loop for continuously running the app
            while (true)
            {
                //showing all the options present 
                Console.WriteLine("-----------------------------");
                Console.WriteLine();
                Console.WriteLine("ENTER 1 FOR ADDING USER");
                Console.WriteLine("ENTER 2 FOR DELETING USER");
                Console.WriteLine("ENTER 3 FOR FETCHING USER BASED ON EMAIL");
                Console.WriteLine("ENTER 4 FOR FETCHING USER BASED ON PHONE NUMBER");
                Console.WriteLine("ENTER 5 FOR FETCHING ALL USERS");
                Console.WriteLine("ENTER 6 FOR SENDING MESSAGE");
                Console.WriteLine("ENTER 7 FOR UNSENDING THE MESSAGE");
                Console.WriteLine("ENTER 8 FOR SHOWING ALL MESSAGES");
                Console.WriteLine("ENTER 9 FOR EXIT");
                Console.WriteLine();
                Console.WriteLine("-----------------------------");

                //used for getting option for the user
                Console.WriteLine("ENTER THE VALUE : ");
                int n = 0;
                while (!int.TryParse(Console.ReadLine(), out n) || n < 1 || n > 9)
                {
                    Console.WriteLine($"ENTER A VALID VALUE");
                }

                if (n == 1) //creating new user
                {

                    //getting users name as input
                    Console.WriteLine("\nENTER YOUT NAME : ");
                    string Name = Console.ReadLine() ?? "";

                    //getting email as input
                    Console.WriteLine("ENTER YOUR EMAIL : ");
                    string Email = Console.ReadLine() ?? "";

                    //getting phone number as input
                    Console.WriteLine("ENTER YOUR PHONE NUMBER : ");
                    string PhoneNumber = Console.ReadLine() ?? "";

                    DateTime CurrentDate = DateTime.Now;

                    User.CreateUser(Name, Email, PhoneNumber, CurrentDate);
                }

                else if (n == 2) //deleting the user
                {
                    //getting user id from user for deleting
                    Console.WriteLine("\nENTER USER ID FOR DELETING : ");
                    int id;
                    while (!int.TryParse(Console.ReadLine(), out id))
                    {
                        Console.WriteLine("INVALID USED ID . PLEASE ENTER A VALID ONE");
                    }

                    User.DeleteUser(id); //calling the method
                }

                else if (n == 3) //fetching details based on email
                {
                    //getting email input from user for fetching user details
                    Console.WriteLine("\nENTER YOUR EMAIL FOR FETCHING DETAILS : ");
                    string Email = Console.ReadLine() ?? "";

                    User.GetUserDetailsByEmail(Email); //calling the method
                }
                else if (n == 4) //fetching details based on phone number
                {
                    //getting phone number input from user for fetching details
                    Console.WriteLine("\nENTER YOUR PHONE NUMBER : ");
                    string PhoneNumber = Console.ReadLine() ?? "";

                    User.GetUserDetailsByPhone(PhoneNumber); //calling the fetch method
                }
                else if (n == 5) //fetching all the user details
                {
                    Console.WriteLine();
                    User.GetAllUserDetails(); //calling the user details method for showing all user details
                }
                else if (n == 6) //method for sending message
                {
                    //getting message input from user
                    Console.WriteLine("\nENTER THE MESSAGE FOR SENDING : ");
                    string Message = Console.ReadLine() ?? "";

                    //showing all the available modes
                    Console.WriteLine("BELOW ARE THE OPTIONS AVAILABLE FOR MODE OF COMMUNICATION");
                    Console.WriteLine("ENTER 1 FOR EMAIL NOTIFICATION : ");
                    Console.WriteLine("ENTER 2 FOR SMS NOTIFICATION : ");

                    //getting input for the desired mode
                    Console.WriteLine("ENTER THE VALUE : ");
                    int value = 0;
                    while (!int.TryParse(Console.ReadLine(), out value) || (value != 1 && value != 2))
                    {
                        Console.WriteLine("PLEASE ENTER A VALID NUMBER FOR MODE OF COMMUNICATION");
                    }

                    Notification.SendNotification(Message, value);//calling the method

                }
                else if (n == 7) //method for unsending message
                {
                    //getting message id from the user
                    Console.WriteLine("\nENTER MESSAGE ID FOR UNSENDING : ");
                    int id;
                    while (!int.TryParse(Console.ReadLine(), out id))
                    {
                        Console.WriteLine("INVALID MESSAGE ID. PLEASE ENTER A VALID ONE");
                    }

                    Notification.UnSendNotification(id); //calling the unsend message method
                }
                else if (n == 8) //fetching all messages
                {
                    Console.WriteLine();
                    Notification.ShowAllMessages(); //calling the method to show all the sent message
                }
                else if (n == 9) //exit the loop
                {
                    break;
                }
            }
        }
    }
}