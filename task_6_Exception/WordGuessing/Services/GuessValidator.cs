
using WordGuessing.Exceptions;
using WordGuessing.Interfaces;

namespace WordGuessing.Services
{
    class GuessValidator : IGuessValidator
    {
        //validate if the guess input from user is valid or not 
        public bool ValidateGuess(string guess)
        {
            if (string.IsNullOrWhiteSpace(guess)) //check the string is null and not empty
            {
                throw new InvalidInputException("PLEASE ENTER A GUESS !");
            }
            else if (guess.Length < 5) //check the string not has less then five characters
            {
                throw new InvalidInputException("THE GUESS SHOULD CONTAIN 5 LETTERS !");
            }
            else if (guess.Length > 5) //check the string not have more than five characters
            {
                throw new InvalidInputException("THE GUESS SHOULD NOT EXCEED 5 LETTERS !");
            }
            else if (guess.Any(char.IsDigit)) //check the string not have any numbers
            {
                throw new InvalidInputException("NUMBERS ARE NOT ALLOWED !");
            }
            else if (guess.Any(c => !char.IsLetter(c))) //check the string has special characters
            {
                throw new InvalidInputException("SPECIAL CHARACTERS ARE NOT ALLOWED !");
            }
            return true;
        }
    }
}