using NotificationSystem.Services;

namespace NotificationSystem
{
    class Program
    {
        static NotificationService notification = new NotificationService();

        static UserService user = new UserService();

        //method for calling create user and getting user inputs
        public static void CreateUser()
        {
            Console.WriteLine("PLEASE ENTER NAME : ");
            string Name = Console.ReadLine() ?? ""; //getting name as an input

            Console.WriteLine("PLEASE ENTER EMAIL : ");
            string Email = Console.ReadLine() ?? ""; //getting email as an input

            Console.WriteLine("PLEASE ENTER PHONE NUMBER : ");
            string Phone = Console.ReadLine() ?? ""; //getting phone number as an input 

            DateTime CurrentDate = DateTime.Now; //storing current date and time 

            var result = user.CreateUser(Name, Email, Phone, CurrentDate); //calling the method 

            if (result != null)
            {
                Console.WriteLine("USER CREATED SUCCESSFULLY !");
            }
            else
            {
                Console.WriteLine("USER CREATION FAILED! PLEASE TRY AGAIN LATER");
            }
        }

        //method for calling delete method and showing result
        public static void DeleteUser()
        {
            Console.WriteLine("PLEASE ENTER USER ID FOR DELETION : ");
            int id = 0;
            while (!int.TryParse(Console.ReadLine(), out id) || (id < 0)) //getting user id as input 
            {
                Console.WriteLine("PLEASE ENTER A VALID USER ID");
            }

            var result = user.DeleteUser(id); //calling delete method for deletion 

            if (result != null)
            {
                Console.WriteLine("USER DELETED SUCCESSFULLY !");
            }
            else
            {
                Console.WriteLine("NO USER WITH THE USER ID EXISTS IN THE DATABASE");
            }
        }

        //method for calling method for fetching user details based on user id
        public static void FetchUser()
        {
            Console.WriteLine("PLEASE ENTER USER ID FOR FETCHING : ");
            int id = 0;
            while (!int.TryParse(Console.ReadLine(), out id) || (id < 0)) //getting user id from user
            {
                Console.WriteLine("PLEASE ENTER A VALID USER ID");
            }

            var result = user.GetUserDetails(id); //calling the method

            if (result != null)
            {
                //showing result
                Console.WriteLine($"{result.UserId} -  {result.Name} -  {result.Email} - +91-{result.PhoneNumber}");
            }
            else
            {
                Console.WriteLine("NO USER WITH THE MENTIONED USER ID EXISTS IN THE DATABASE");
            }
        }

        //method for calling user service method for fetching all users 
        public static void FetchAllUser()
        {
            var results = user.GetAllUserDetails();

            if (results != null)
            {
                //showing all the user details
                foreach (var result in results)
                {
                    Console.WriteLine($"{result.UserId} -  {result.Name} -  {result.Email} - +91-{result.PhoneNumber}");
                }
            }
            else
            {
                Console.WriteLine("NO USER WITH THE MENTIONED USER ID EXISTS IN THE DATABASE");
            }
        }


        //method for calling the update method and getting user inputs
        public static void UpdateUser()
        {
            Console.WriteLine("PLEASE ENTER USER ID FOR UPDATING : ");
            int id = 0;
            while (!int.TryParse(Console.ReadLine(), out id) || (id < 0)) //getting user id 
            {
                Console.WriteLine("PLEASE ENTER A VALID USER ID");
            }

            Console.WriteLine("PLEASE ENTER NAME : ");
            string Name = Console.ReadLine() ?? ""; // getting name 

            Console.WriteLine("PLEASE ENTER EMAIL : ");
            string Email = Console.ReadLine() ?? ""; //getting email

            Console.WriteLine("PLEASE ENTER PHONE NUMBER : ");
            string Phone = Console.ReadLine() ?? ""; // getting phone 

            DateTime CurrentDate = DateTime.Now; //getting currentTime

            var result = user.UpdateUserDetails(id, Name, Email, Phone, CurrentDate);

            if (result != null)
            {
                //showing status
                Console.WriteLine("USER CREATED SUCCESSFULLY !");
            }
            else
            {
                Console.WriteLine("USER CREATION FAILED! PLEASE TRY AGAIN LATER");
            }
        }

        //method for calling create notification and getting input values
        public static void SendNotification()
        {
            Console.WriteLine("PLEASE MESSAGE FOR SENDING : ");
            string Message = Console.ReadLine() ?? ""; // getting message for sending 

            Console.WriteLine("BELOW ARE THE AVAILABLE MODES FOR COMMUNICATION ");
            Console.WriteLine("ENTER 1 FOR EMAIL NOTIFICATION ");
            Console.WriteLine("ENTER 2 FOR SMS NOTIFICATION ");
            Console.WriteLine("PLEASE ENTER THE VALUE : ");
            int value = 0;
            while (!int.TryParse(Console.ReadLine(), out value) && (value != 1 || value != 2)) //getting notification type 
            {
                Console.WriteLine("PLEASE ENTER A VALID VALUE");
            }

            Console.WriteLine("PLEASE ENTER USER ID FOR SENDING: ");
            int id = 0;
            while (!int.TryParse(Console.ReadLine(), out id) || (id < 0)) //getting user id 
            {
                Console.WriteLine("PLEASE ENTER A VALID USER ID");
            }

            var result = notification.SendNotification(Message, value, id);

            Console.WriteLine(Message , value , id);

            if (result != null)
            {
                Console.WriteLine($"{result.NotificationType} SENDED SUCCESSFULLY !");
            }
            else
            {
                Console.WriteLine($"SENDING FAILED! PLEASE TRY AGAIN LATER");
            }
        }

        //method for getting user id and calling the unsent notification method
        public static void UnsentNotification()
        {
            Console.WriteLine("PLEASE ENTER MESSAGE ID FOR UNSENDING : ");
            int id = 0;
            while (!int.TryParse(Console.ReadLine(), out id) || (id < 0)) //getting message id as input 
            {
                Console.WriteLine("PLEASE ENTER A VALID MESSAGE ID");
            }

            var result = notification.UnsentNotification(id); //calling delete method for deletion 

            if (result != null)
            {
                Console.WriteLine("MESSAGE UN SENDED SUCCESSFULLY !");
            }
            else
            {
                Console.WriteLine("NO MESSAGE WITH THE MESSAGE ID EXISTS IN THE DATABASE");
            }
        }

        //method for calling method for fetching notification based on user id
        public static void GetNotification()
        {
            Console.WriteLine("PLEASE ENTER MESSAGE ID FOR FETCHING : ");
            int id = 0;
            while (!int.TryParse(Console.ReadLine(), out id) || (id < 0)) //getting message id from user
            {
                Console.WriteLine("PLEASE ENTER A VALID MESSAGE ID");
            }

            var result = notification.GetNotification(id); //calling the method

            if (result != null)
            {
                //showing result
                Console.WriteLine($"{result.MessageId} - {result.Message} -  {result.SendedAt} - {result.NotificationType}");
            }
            else
            {
                Console.WriteLine("NO MESSAGE WITH THE MENTIONED MESSAGE ID EXISTS IN THE DATABASE");
            }
        }

        //method for fetching all notification
        public static void GetAllNotification()
        {
            var results = notification.GetAllNotification();

            if (results != null)
            {
                //showing all the user details
                foreach (var result in results)
                {
                    Console.WriteLine($"{result.MessageId} -  {result.Message} -  {result.SendedAt} - {result.NotificationType}");
                }
            }
            else
            {
                Console.WriteLine("NO MESSAGES EXISTS IN THE DATABASE");
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
                Console.WriteLine("ENTER 8 FOR FETCHING A NOTIFICATION");
                Console.WriteLine("ENTER 9 FOR FETCHING ALL NOTIFICATION");
                Console.WriteLine("ENTER 0 FOR EXIT");
                Console.WriteLine();
                Console.WriteLine("------------------------------------------");


                Console.WriteLine("PLEASE ENTER THE VALUE : ");
                int n = 0;
                while (!int.TryParse(Console.ReadLine(), out n) || (n < 0 || n > 9))
                {
                    Console.WriteLine("PLEASE ENTER A VALID NUMBER");
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
                else if(n == 6) //sending notification
                {
                    SendNotification();
                }
                else if( n == 7) //unsending notification
                {
                     UnsentNotification();
                }
                else if(n == 8) // fetching message by id
                {
                     GetNotification();
                }
                else if(n == 9) //fetch all notification
                {
                    GetAllNotification();
                }
                else if(n == 0) // exit console
                {
                    break;
                }
            }
        }
    }
}