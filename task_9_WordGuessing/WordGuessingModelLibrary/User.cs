namespace WordGuessingModelLibrary
{
    public class Users
    {
        //variable for storing username
        public string UserName { get; set; } = string.Empty;

        //variable for storing password
        public string Password { get; set; } = string.Empty;

        //variable for storing createdat
        public DateTime CreatedAt { get; set; }

        //Constructor to initialize variable
        public Users(string username, string password, DateTime createdat)
        {
            UserName = username;
            Password = password;
            CreatedAt = createdat;
        }

        public Users()
        {
            
        }
    }
}