using WordGuessingModelLibrary;

namespace WordGuessingBLLibrary.Interfaces
{
    //interface for game class
    public interface IGame
    {
        public void StartGame(); //method used for starting the game

        public string ValidatingGuess(string username, string guess, int attempt); //method used for validating a guess

        public void CreateResult(string UserName, int attempt, string word, int score); //method for storing result

        public List<Leader>? LeaderBoard(); //method for returning a LeaderBoard

        public List<Results>? ShowReplay(string username); //method for showing all replay
    }
}