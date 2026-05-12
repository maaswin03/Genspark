namespace WordGuessingModelLibrary
{
    public class Guess
    {
        //variable for storing username
        public string UserName { get; set; }

        //variable for storing the attempt number
        public int Attempt { get; set; }

        //variable for storing the Guessed Word
        public string GuessedWord { get; set; }

        //variable for storing is the guess is correct or Wrong
        public bool IsCorrect { get; set; }

        //variable for feedback 
        public string FeedBack { get; set; }

        //Constructor for the class
        public Guess(string username, int attempt, string guess, bool iscorrect, string feedback)
        {
            UserName = username;
            Attempt = attempt;
            GuessedWord = guess;
            IsCorrect = iscorrect;
            FeedBack = feedback;
        }

    }
}