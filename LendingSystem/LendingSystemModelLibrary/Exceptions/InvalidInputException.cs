namespace LendingSystemModelLibrary.Exceptions
{
    public class InvalidInputException : Exception
    {
        private string _message;

        public InvalidInputException(string message)
        {
            _message = message;
        }

        public override string Message => _message;
    }
}