using WordGuessing.Services;
using WordGuessing.Exceptions;
using WordGuessing.Interfaces;
namespace WordGuessing
{
    class Program
    {

        static IGame game = new Game();

        public static void StartGame()
        {
            Dictionary<int, string> attemptComments = new Dictionary<int, string>() { { 1, "GENIUS!" }, { 2, "EXCELLENT!" }, { 3, "GREAT JOB!" }, { 4, "GOOD WORK!" }, { 5, "NICE TRY!" }, { 6, "THAT WAS CLOSE!" } };
            game.StartGame();
            bool isWon = false;

            for (int i = 0; i < 6; i++)
            {
                try
                {
                    Console.WriteLine("\nPLEASE ENTER YOUR GUESS : ");
                    string guess = Console.ReadLine() ?? "";

                    var result = game.ValidatingGuess(guess, i + 1);

                    if (result == "GGGGG")
                    {
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine("-------------------------------------------------------");
                        Console.WriteLine();
                        int score = (6 - i) * 20;
                        Console.WriteLine($"\n{attemptComments[i + 1]}");
                        Console.WriteLine("CONGRATULATIONS! YOU GUESSED THE WORD CORRECTLY");
                        Console.WriteLine($"YOU COMPLETED THE GAME IN {i + 1} ATTEMPTS");
                        Console.WriteLine($"YOUR SCORE : {score}/120");
                        Console.WriteLine();
                        Console.ResetColor();
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
            }
        }

        public static void ShowReplay()
        {
            var result = game.ShowReplay();
            if (result != null && result.Count > 0)
            {
                foreach (var item in result)
                {
                    Console.WriteLine($"{item.Attempt} - {item.GuessedWord} - {item.FeedBack}");
                }
            }
            else
            {
                Console.WriteLine("OOPS! NO REPLAY TO SHOW PLEASE TAKE A NEW GAME");
            }
        }

        public static void Main(String[] args)
        {
            while (true)
            {
                //list of options available 
                Console.WriteLine("-------------------------------------------------------");
                Console.WriteLine();
                Console.WriteLine("ENTER 1 FOR STARTING A NEW GAME");
                Console.WriteLine("ENTER 2 FOR SEEING THE PREVIOUS GAME REPLAY");
                Console.WriteLine("ENTER 3 FOR QUIT");
                Console.WriteLine();
                Console.WriteLine("-------------------------------------------------------");

                //getting input from the user
                Console.WriteLine("\nPLEASE ENTER THE OPTION : ");
                int n = 0;
                while (!int.TryParse(Console.ReadLine(), out n) || (n < 0 || n > 3))
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
                    break;
                }
            }
        }
    }
}