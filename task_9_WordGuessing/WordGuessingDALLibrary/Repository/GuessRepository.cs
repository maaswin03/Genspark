using Npgsql;
using WordGuessingDALLibrary.Interfaces;
using WordGuessingModelLibrary;

namespace WordGuessingDALLibrary.Repository
{
    //this acts as session storage for storing guess and attempts
    public class GuessRepository : IRepository<string, Guess>
    {
        List<Guess> _guessMap = new List<Guess>(); //list for storing all the attempt details

        //method for storing the guess 
        public Guess? Create(Guess guess)
        {
            if (_guessMap.Count < 6) //checking the list consist guess of less than 6
            {
                _guessMap.Add(guess); //adding to list
                return guess; //returning guess
            }
            return null;
        }

        //method for deleting the contents of the list
        public List<Guess>? DeleteAllData()
        {
            if (_guessMap.Count > 0) //check if the list reaches maximum attempt
            {
                var list = _guessMap;
                _guessMap.Clear(); //clearing all the values
                return list;
            }
            return null;
        }

        //method for checking if the current guessed Word is already present
        public Guess? GetData(string key)
        {
            if (_guessMap.Count > 0)
            {
                foreach (var item in _guessMap)
                {
                    if (item.GuessedWord == key)
                    {
                        return item;
                    }
                }
            }
            return null;
        }
    }
}