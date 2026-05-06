using WordGuessing.Models;

namespace WordGuessing.Interfaces
{
    //interface for game class
    interface IGame
    {
        public void StartGame(); //method used for starting the game

        public string ValidatingGuess(string guess, int attempt); //method used for validating a guess

        public List<Guess>? ShowReplay(); //method for showing all replay
    }
}