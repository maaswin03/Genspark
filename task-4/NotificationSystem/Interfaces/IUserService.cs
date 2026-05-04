using NotificationSystem.Models;

namespace NotificationSystem.Interfaces
{
    interface IUserService
    {
        void CreateUser(string Name, string Email, string PhoneNumber, DateTime CreatedA); //method for creating new user

        void DeleteUser(int id); //method for deleting user

        int GetUserDetailsByEmail(string Email); //method for fetching user by email

        int GetUserDetailsByPhone(string Phone); //method for fetching user by phone

        void GetAllUserDetails(); //method for fetching all user details

    }
}