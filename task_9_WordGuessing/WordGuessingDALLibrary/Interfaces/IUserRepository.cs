using WordGuessingModelLibrary;

namespace WordGuessingDALLibrary.Interfaces
{
    public interface IUserRepository
    {
        public bool Create(Users user); //method for creating new user

        public bool GetData(Users user); //method for getting the user with the username and password present
        
        public bool CheckUser(string username); //method for checking username exists
    }
}