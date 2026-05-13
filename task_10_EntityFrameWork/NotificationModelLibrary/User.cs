using System.ComponentModel.DataAnnotations.Schema;

namespace NotificationModelLibrary
{
    public class User
    {
        //variable for storing user id
        public int UserId { get; set; }

        //variable for storing user name 
        public string Name { get; set; } = string.Empty;
        
        //variable for storing user email
        public string Email { get; set; } = string.Empty;

        //variable for storing user phone number
        public string PhoneNumber { get; set; } = string.Empty;

        //variable for storing user account creation date
        public DateTime CreatedAt { get; set; }


        public ICollection<Notification>? Notifications { get; set; }

        public User()
        {

        }

        //constructor to set values
        public User(string name, string email, string phone, DateTime createdAt)
        {
            Name = name;
            Email = email;
            PhoneNumber = phone;
            CreatedAt = createdAt;
        }

        //method fo printing details when printing the obj
        public override string ToString()
        {
            return $"{Name} - {Email} - {PhoneNumber} - {CreatedAt}";
        }
    }
}