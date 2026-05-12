using WordGuessingBLLibrary.Interfaces;
using WordGuessingDALLibrary.Repository;
using WordGuessingDALLibrary.Interfaces;
using WordGuessingModelLibrary;
using WordGuessingModelLibrary.Exceptions;

namespace WordGuessingBLLibrary.Services
{
    public class UserService : IUserService
    {
        IUserRepository _user;

        //constructor for initializing user service
        public UserService()
        {
            _user = new UserRepository();
        }

        //method for checking the user already exists or create a new user
        public bool Authenticate(string username, string password)
        {
            Users user = new Users(username, password, DateTime.Now);
            var userExists = _user.CheckUser(username); //check the username already exists

            if (userExists) //if exists
            {
                return _user.GetData(user); //check username and password
            }
            //if username not exists and validate password successful
            else if (!userExists && Validate(password))
            {
                return _user.Create(user);
            }
            return false;
        }

        public bool Validate(string password)
        {
            if (password.Length < 8)
            {
                throw new InvalidInputException("PLEASE ENTER A STRONG PASSWORD");
            }
            return true;
        }

    }
}