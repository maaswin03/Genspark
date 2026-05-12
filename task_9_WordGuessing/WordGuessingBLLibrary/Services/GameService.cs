using WordGuessingModelLibrary;
using WordGuessingDALLibrary.Repository;
using WordGuessingDALLibrary.Interfaces;
using WordGuessingModelLibrary.Exceptions;
using WordGuessingBLLibrary.Interfaces;
using WordGuessingBLLibrary.Gamelogic;

namespace WordGuessingBLLibrary.Services
{
    public class Game : IGame
    {
        private GuessRepository _repository;

        private IResultRepository _resrepository;

        private WordProvider _wordprovider;

        private GuessValidator _guessvalidator;

        private FeedBackGenerator _feedbackgenerator;

        private string _original = "";

        public Game()
        {
            _repository = new GuessRepository();
            _wordprovider = new WordProvider();
            _guessvalidator = new GuessValidator();
            _feedbackgenerator = new FeedBackGenerator();
            _resrepository = new ResultRepository();
        }

        //method to get hidden word and delete the existing guesses
        public void StartGame()
        {
            _original = _wordprovider.GenerateRandomWord(); //getting word
            _repository.DeleteAllData();
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

        //method for creating final results 
        public void CreateResult(string UserName, int attempt, string word, int score)
        {
            Results result = new Results(UserName, attempt, word, score , DateTime.Now);
            _resrepository.Create(result);
        }

        //method for showing LeaderBoard
        public List<Leader>? LeaderBoard()
        {
            return _resrepository.GetData();
        }

        //method for validating and checking the guess
        public string ValidatingGuess(string username, string guess, int attempt)
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

            Guess item = new Guess(username, attempt, guess, IsCorrect, feedback); //create an object

            _repository.Create(item); //store in the list

            return feedback;
        }

        //method for fetching all the replay
        public List<Results>? ShowReplay(string username)
        {
            return _resrepository.GetUserData(username);
        }
    }
}
