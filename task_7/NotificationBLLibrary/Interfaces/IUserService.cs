using NotificationModelLibrary;

namespace NotificationBLLibrary.Interfaces
{
    public interface IUserService
    {
        public User? CreateUser(string name, string email, string phonenumber, DateTime createddate); //method for creating new user

        public User? DeleteUser(int id); //method for deleting user

        public User? GetUserDetails(int id); //method for getting particular user

        public List<User>? GetAllUserDetails(); //method for fetching all user details

        public User? UpdateUserDetails(int id, string name, string email, string phonenumber, DateTime createddate); //method for updating all user details

    }
}