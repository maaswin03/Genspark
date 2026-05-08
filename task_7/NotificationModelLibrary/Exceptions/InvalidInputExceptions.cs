namespace NotificationModelLibrary.Exceptions
{
    public class InvalidInputExceptions : Exception
    {
        //private value to store the message
        private string _message;

        //constructor to set the message
        public InvalidInputExceptions(string message)
        {
            _message = message;
        }

        public override string Message => _message;
    }
}