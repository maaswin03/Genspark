using NotificationSystem.Models;
using NotificationSystem.Interfaces;

namespace NotificationSystem.Services
{
    class UserService : IUserService
    {
        List<User> UserStack = new List<User>();

        public int LastUserid = 0;


        //method for creating a new user
        public void CreateUser(string name, string email, string phonenumber, DateTime createdat)
        {
            User user = new User(name, email, phonenumber, createdat); //creating an object with the help of the user constructor
            LastUserid++;
            user.UserId = LastUserid; //creating a unique user id
            if (ValidateUser(user)) // to validate the user inputs before creating documents
            {
                UserStack.Add(user);
                Console.WriteLine("USER CREATED SUCCESSFULLY !");
            }
            else
            {
                Console.WriteLine("USER CREATION FAILED ! PLEASE RETRY AGAIN");
            }
        }

        //method for deleting user 
        public void DeleteUser(int id)
        {
            var user = UserStack.FirstOrDefault(x => x.UserId == id);// method for getting the id and user object 
            if (user != null) //if not null delete the user
            {
                UserStack.Remove(user);
                Console.WriteLine("THE USER DETAIL IS DELETED SUCCESSFULLY !");
            }
            else
            {
                Console.WriteLine("THE USER DETAIL WITH THAT ID IS NOT PRESENT");
            }
        }
        //method to fetch user details based on email
        public int GetUserDetailsByEmail(string Email)
        {
            User? user = null;
            foreach (var item in UserStack)
            {
                if (item.Email == Email) //checking the email id is present
                {
                    user = item;
                    break;
                }
            }
            if (user != null)
            {
                PrintDetails(user); // calling the method for printing details
                return user.UserId;
            }
            else
            {
                Console.WriteLine("NO ACCOUNT WITH THE GIVEN EMAIL ID IS PRESENT. PLEASE ENTER VALID EMAIL");
                return 0;
            }
        }

        //method to fetch user details based on user id
        public int GetUserDetailsByPhone(string phone)
        {
            User? user = null;
            foreach (var item in UserStack)
            {
                if (item.PhoneNumber == phone) //checking the user id is present
                {
                    user = item;
                    break;
                }
            }
            if (user != null)
            {
                PrintDetails(user); // calling the method for printing details
                return user.UserId;
            }
            else
            {
                Console.WriteLine("NO ACCOUNT WITH THE GIVEN USER ID IS PRESENT. PLEASE ENTER VALID USER ID");
                return 0;
            }
        }


        //method for fetching all user details
        public void GetAllUserDetails()
        {
            if (UserStack.Count == 0)
            {
                Console.WriteLine("NO USER PRESENT CURRENTLY . PLEASE ADD A NEW USER");
            }
            else
            {
                Console.WriteLine("BELOW ARE THE USER PRESENT IN THE DATABASE");
                foreach (var item in UserStack)
                {
                    PrintDetails(item);
                }
            }
        }

        //method for validating the user input
        public bool ValidateUser(User user)
        {
            //checking the phone number has 10 digits
            if (user.PhoneNumber.Length != 10)
            {
                Console.WriteLine("PLEASE ENTER A VALID PHONE NUMBER");
                return false;
            }
            //checking the email is valid
            else if (!user.Email.Contains("@") || !user.Email.Contains("."))
            {
                Console.WriteLine("PLEASE ENTER A VALID EMAIL");
                return false;
            }
            return true; // return true if all things get satisfied
        }

        public void PrintDetails(User user)
        {
            Console.WriteLine("-----------------------------");
            Console.WriteLine(user);
            Console.WriteLine("-----------------------------");
        }
    }
}