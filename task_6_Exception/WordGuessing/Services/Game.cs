using WordGuessing.Models;
using WordGuessing.Repository;
using WordGuessing.Exceptions;
using WordGuessing.Interfaces;

namespace WordGuessing.Services
{
    class Game : IGame
    {
        private GuessRepository _repository;
        private IWordProvider _wordprovider;

        private IGuessValidator _guessvalidator;

        private IFeedBackGenerator _feedbackgenerator;

        private string _original = "";

        public Game()
        {
            _repository = new GuessRepository();
            _wordprovider = new WordProvider();
            _guessvalidator = new GuessValidator();
            _feedbackgenerator = new FeedBackGenerator();
        }

        //method to get hidden word and delete the existing guesses
        public void StartGame()
        {
            _original = _wordprovider.GenerateRandomWord(); //getting word
            _repository.DeleteAllData(); //delete values

        }

        //method for getting is the guess already present
        public bool IsGuessAvailable(string guess)
        {
            var result = _repository.GetData(guess);
            if (result == null)
            {
                return true;
            }
            return false;
        }

        //method for validating and checking the guess
        public string ValidatingGuess(string guess, int attempt)
        {
            guess = guess.ToUpper(); //convert to uppercase

            //validate the guess
            _guessvalidator.ValidateGuess(guess);

            //check guess is present
            if (!IsGuessAvailable(guess))
            {
                throw new InvalidInputException("THE GUESS YOU ENTERED IS ALREADY USED !");
            }

            string feedback = _feedbackgenerator.GuessFeedback(_original, guess); //get feedback
            bool IsCorrect = feedback == "GGGGG"; //check the guess is correct or not

            Guess item = new Guess(attempt, guess, IsCorrect, feedback); //create an object

            _repository.Create(item); //store in the list

            return feedback;
        }

        //method for fetching all the replay
        public List<Guess>? ShowReplay()
        {
            return _repository.GetAllData();
        }
    }
}
