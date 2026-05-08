using NotificationDALLibrary.Repository;
using NotificationModelLibrary.Exceptions;
using NotificationModelLibrary;
using NotificationBLLibrary.Interfaces;

namespace NotificationBLLibrary.Services
{
    public class UserService : IUserService
    {

        UserRepository _userrepo = new UserRepository();

        //method for validating the user input and creating a user
        public User? CreateUser(string name, string email, string phonenumber, DateTime createddate)
        {
            User user = new User(name, email, phonenumber, createddate);
            if (ValidateUser(user))
            {
                return _userrepo.Create(user);
            }
            return null;
        }

        //method for validating and deleting user 
        public User? DeleteUser(int id)
        {
            if (id > 0)
            {
                return _userrepo.Delete(id);
            }
            return null;
        }

        public User? GetUserDetails(int id)
        {
            if (id > 0)
            {
                return _userrepo.GetData(id);
            }
            return null;
        }


        //method for fetching all user details
        public List<User>? GetAllUserDetails()
        {
            return _userrepo.GetAllData();
        }

        //method for updating the user details()
        public User? UpdateUserDetails(int id, string name, string email, string phonenumber, DateTime createddate)
        {
            User user = new User(name, email, phonenumber, createddate);
            if (ValidateUser(user) && id > 0)
            {
                return _userrepo.Update(id, user);
            }
            return null;
        }

        //method for validating the user input
        public bool ValidateUser(User user)
        {
            var allUsers = _userrepo.GetAllData();
            bool UserEmail = false;
            bool UserPhone = false;

            //check existing details
            if (allUsers != null)
            {
                //using LINQ for checking email or phone already present
                UserEmail = allUsers.Any(x => x.Email == user.Email);
                UserPhone = allUsers.Any(x => x.PhoneNumber == user.PhoneNumber);
            }
            if (user.Name == "")
            {
                throw new InvalidInputExceptions("PLEASE ENTER A VALID NAME !");
            }
            //checking the phone number has 10 digits
            else if (user.PhoneNumber.Length != 10)
            {
                throw new InvalidInputExceptions("PLEASE ENTER A VALID PHONE NUMBER !");
            }
            //checking the email is valid
            else if (!user.Email.Contains("@") || !user.Email.Contains("."))
            {
                throw new InvalidInputExceptions("PLEASE ENTER A VALID EMAIL ID !");
            }
            //validate existing email
            else if (UserEmail)
            {
                throw new InvalidInputExceptions("EMAIL ALREADY EXISTS WITH US ENTER A NEW ONE !");
            }
            //validating existing phone number
            else if (UserPhone)
            {
                throw new InvalidInputExceptions("PHONE NUMBER ALREADY EXISTS WITH US ENTER A NEW ONE !");
            }
            return true; // return true if all things get satisfied
        }

    }
}