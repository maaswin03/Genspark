namespace WordGuessingModelLibrary
{
    public class Results
    {
        //variable for storing UserName
        public string UserName { get; set; } = string.Empty;

        //variable fos storing attempt
        public int Attempt { get; set; }

        //variable for storing word
        public string Word { get; set; } = string.Empty;

        //variable for storing Score
        public int Score { get; set; }

        public DateTime PlayedAt {get; set;}

        public Results(string username, int attempt, string word, int score , DateTime playedat)
        {
            UserName = username;
            Attempt = attempt;
            Word = word;
            Score = score;
            PlayedAt = playedat;
        }

        public Results()
        {
            
        }

    }
}