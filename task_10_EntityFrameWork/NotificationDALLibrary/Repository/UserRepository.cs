using NotificationDALLibrary.Context;
using NotificationDALLibrary.Interface;
using NotificationModelLibrary;
using NotificationModelLibrary.Exceptions;
using Npgsql;

namespace NotificationDALLibrary.Repository
{
    public class UserRepository : IRepository<int, User>
    {

        NotificationSystemContext context;

        //initializing context
        public UserRepository()
        {
            context = new NotificationSystemContext();
        }

        //method for inserting into user table
        public User Create(User item)
        {

            context.users.Add(item); //add the user
            context.SaveChanges(); //SaveChanges into the database
            return item;
        }

        //method for deleting a user
        public User? Delete(int key)
        {

            var user = context.users.FirstOrDefault(u => u.UserId == key); //get the user details
            if (user == null) //check if the user exists 
            {
                return null;
            }
            context.users.Remove(user); //remove the user 
            context.SaveChanges(); //save the changes in database
            return user;
        }

        //method for getting particular user details
        public User? GetData(int key)
        {
            return context.users.FirstOrDefault(u => u.UserId == key); //get the user details
        }

        //method for returning all the existing user 
        public List<User>? GetAllData()
        {
            return context.users.ToList(); //return the user list
        }

        //method for updating the user details
        public User? Update(int key, User item)
        {

            var existing_user = context.users.FirstOrDefault(u => u.UserId == key); //getting the user details

            if (existing_user == null) //checking the user is not present
            {
                return null;
            }

            //update the use details
            existing_user.Name = item.Name;
            existing_user.Email = item.Email;
            existing_user.PhoneNumber = item.PhoneNumber;
            context.SaveChanges(); //SaveChanges into the table
            return item; //return item
        }
    }
}