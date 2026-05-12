using WordGuessingModelLibrary;

namespace WordGuessingBLLibrary.Interfaces
{
    public interface IUserService
    {
        //method to check the user already present 
        public bool Authenticate(string username , string password);
    }
}