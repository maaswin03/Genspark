namespace WordGuessingModelLibrary.Exceptions
{
    public class InvalidInputException : Exception
    {
        private string _message; //private variable to store message

        //constructor to set value to the message
        public InvalidInputException(string message)
        {
            _message = message;
        }

        public override string Message => _message;
    }
}