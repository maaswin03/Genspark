namespace NotificationSystem.Models
{
    class User
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

        public override string ToString()
        {
            return $"{UserId} - {Name} - {Email} - {PhoneNumber}";
        }
    }
}