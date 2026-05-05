using NotificationSystem.Models;
using NotificationSystem.Interfaces;
using NotificationSystem.Repository;

namespace NotificationSystem.Services
{
    class UserService : IUserService
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
                return _userrepo.Update(id,user);
            }
            return null;
        }

        //method for validating the user input
        public bool ValidateUser(User user)
        {
            //checking the phone number has 10 digits
            if (user.PhoneNumber.Length != 10)
            {
                return false;
            }
            //checking the email is valid
            else if (!user.Email.Contains("@") || !user.Email.Contains("."))
            {
                return false;
            }
            return true; // return true if all things get satisfied
        }

    }
}