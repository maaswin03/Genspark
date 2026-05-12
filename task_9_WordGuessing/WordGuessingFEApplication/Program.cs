using WordGuessingBLLibrary.Services;
using WordGuessingModelLibrary.Exceptions;
using WordGuessingBLLibrary.Interfaces;

namespace WordGuessingFEApplication
{
    class Program
    {

        static IGame game = new Game();

        static IUserService user = new UserService();

        static string UserName = "";

        //method for creating a new game
        public static void StartGame()
        {
            Dictionary<int, string> attemptComments = new Dictionary<int, string>() { { 1, "GENIUS!" }, { 2, "EXCELLENT!" }, { 3, "GREAT JOB!" }, { 4, "GOOD WORK!" }, { 5, "NICE TRY!" }, { 6, "THAT WAS CLOSE!" } };
            game.StartGame(); //starting the game
            bool isWon = false;

            for (int i = 0; i < 6; i++)
            {
                try
                {
                    Console.WriteLine("\nPLEASE ENTER YOUR GUESS : ");
                    string guess = Console.ReadLine() ?? "";

                    //validating the user input
                    var result = game.ValidatingGuess(UserName, guess, i + 1);

                    if (result == "GGGGG")
                    {
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine("-------------------------------------------------------");
                        Console.WriteLine();
                        int score = (6 - i) * 20; //calculate score
                        Console.WriteLine($"\n{attemptComments[i + 1]}");
                        Console.WriteLine("CONGRATULATIONS! YOU GUESSED THE WORD CORRECTLY");
                        Console.WriteLine($"YOU COMPLETED THE GAME IN {i + 1} ATTEMPTS");
                        Console.WriteLine($"YOUR SCORE : {score}/120");
                        Console.WriteLine();
                        Console.ResetColor();

                        //create a result for that game
                        game.CreateResult(UserName, i + 1, guess, score);

                        isWon = true;
                        break;
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine();
                        Console.WriteLine("-------------------------------------------------------");
                        Console.WriteLine();
                        Console.WriteLine("\nINCORRECT GUESS!");
                        Console.WriteLine("FEEDBACK FOR YOUR GUESS : ");
                        Console.WriteLine(result);
                        Console.WriteLine();
                        Console.WriteLine("-------------------------------------------------------");
                        Console.ResetColor();
                    }
                }
                catch (InvalidInputException ex)
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine(ex.Message);
                    Console.WriteLine();
                    Console.WriteLine("-------------------------------------------------------");
                    Console.ResetColor();
                    i--;
                }
            }
            if (!isWon)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("\nGAME OVER!");
                Console.WriteLine("YOU HAVE USED ALL 6 ATTEMPTS");
                Console.WriteLine("YOUR SCORE : 0");
                Console.ResetColor();
                game.CreateResult(UserName, 6, "FAILED TO GUESS", 0);
            }
        }

        //method for displaying the LeaderBoard
        public static void LeaderBoard()
        {
            var result = game.LeaderBoard(); //getting the result
            int i = 1;

            if (result != null && result.Count > 0)
            {
                Console.WriteLine("\nRANK - USERNAME - TOTAL GAMES - TOTAL SCORE - BEST SCORE");
                foreach (var r in result)
                {
                    Console.WriteLine($"{i} - {r.UserName}  -  {r.TotalGames}  -  {r.TotalScore}  -  {r.MaxScore}");
                    i++;
                }
            }
            else
            {
                Console.WriteLine("\nLEADERBOARD IS CURRENTLY EMPTY START A NEW GAME TO REACH SPOT 1");
            }
        }

        //showing the replay of the game
        public static void ShowReplay()
        {
            var result = game.ShowReplay(UserName);
            if (result != null && result.Count > 0)
            {
                Console.WriteLine("TOTAL ATTEMPT - WORD - SCORE - PLAYED AT");
                foreach (var item in result)
                {
                    Console.WriteLine($"{item.Attempt}  -  {item.Word}  -  {item.Score}  -  {item.PlayedAt}");
                }
            }
            else
            {
                Console.WriteLine("OOPS! NO REPLAY TO SHOW PLEASE TAKE A NEW GAME");
            }
        }

        public static void Main(String[] args)
        {
            try
            {
                Console.WriteLine("\nPLEASE ENTER USERNAME/PASSWORD TO LOGIN IN OR SIGN UP");

                Console.WriteLine("\nPLEASE ENTER USERNAME : ");
                UserName = Console.ReadLine() ?? "";

                Console.WriteLine("\nPLEASE ENTER THE PASSWORD : ");
                string password = Console.ReadLine() ?? "";

                if (user.Authenticate(UserName, password))
                {
                    while (true)
                    {
                        //list of options available 
                        Console.WriteLine("-------------------------------------------------------");
                        Console.WriteLine();
                        Console.WriteLine("ENTER 1 FOR STARTING A NEW GAME");
                        Console.WriteLine("ENTER 2 FOR SEEING THE PREVIOUS GAME REPLAY");
                        Console.WriteLine("ENTER 3 FOR SHOWING THE LEADERBOARD");
                        Console.WriteLine("ENTER 4 FOR QUIT");
                        Console.WriteLine();
                        Console.WriteLine("-------------------------------------------------------");

                        //getting input from the user
                        Console.WriteLine("\nPLEASE ENTER THE OPTION : ");
                        int n = 0;
                        while (!int.TryParse(Console.ReadLine(), out n) || (n < 1 || n > 4))
                        {
                            Console.WriteLine("PLEASE ENTER A VALID NUMBER");
                        }
                        Console.WriteLine();

                        //option for entering into the game
                        if (n == 1)
                        {
                            Console.ForegroundColor = ConsoleColor.Cyan;
                            Console.WriteLine("-------------------------------------------------------");
                            Console.WriteLine();
                            Console.WriteLine("BELOW ARE THE RULES TO BE FOLLOWED");
                            Console.WriteLine("1. THE PLAYER SHOULD GUESS A 5 LETTER WORD");
                            Console.WriteLine("2. THE PLAYER WILL GET A MAXIMUM OF 6 ATTEMPTS");
                            Console.WriteLine("3. FEEDBACK RULES:");
                            Console.WriteLine("4. G - CORRECT LETTER IN CORRECT POSITION");
                            Console.WriteLine("5. Y - CORRECT LETTER IN WRONG POSITION");
                            Console.WriteLine("6. X - LETTER NOT PRESENT IN THE WORD");
                            Console.WriteLine("7. ONLY ALPHABETS ARE ALLOWED");
                            Console.WriteLine();
                            Console.WriteLine("-------------------------------------------------------");
                            Console.ResetColor();
                            StartGame();

                        }

                        //option for showing replay
                        else if (n == 2)
                        {
                            ShowReplay();
                        }

                        else if (n == 3)
                        {
                            LeaderBoard();
                        }

                        else if (n == 4)
                        {
                            break;
                        }
                    }
                }
                else
                {
                    Console.WriteLine("\nPLEASE ENTER A VALID USERNAME OR PASSWORD");
                }
            }
            catch (InvalidInputException e)
            {
                Console.WriteLine($"\n{e.Message}");
            }
        }
    }
}