namespace WordGuessing.Interfaces
{
    //interface for guess validator
    interface IGuessValidator
    {
        public bool ValidateGuess(string guess); //method for validating the guess
    }
}