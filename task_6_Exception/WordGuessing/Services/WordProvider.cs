using WordGuessing.Interfaces;

namespace WordGuessing.Services
{
    class WordProvider : IWordProvider
    {

        //List containing all the words used for guessing
        List<string> Word = ["APPLE", "MANGO", "GRAPE", "TRAIN", "PLANT", "BRAIN"];

        //method for returning random words from the list
        public string GenerateRandomWord()
        {
            Random random = new Random();
            int number = random.Next(Word.Count); //getting a random number based on the list size
            return Word[number]; //returning the word present in the random number index
        }
    }
}