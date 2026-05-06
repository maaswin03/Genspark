// using WordGuessing.Interfaces;

// namespace WordGuessing.Services
// {
//     class FeedBackGenerator : IFeedBackGenerator
//     {
//         //method for giving feedback 
//         public string GuessFeedback(string original, string guess)
//         {
//             string FeedBack = "";

//             for (int i = 0; i < 5; i++)
//             {
//                 if (guess[i] == original[i]) //if the letter exists in the same index as the original.
//                 {
//                     FeedBack += "G";
//                 }
//                 else if (original.Contains(guess[i])) //the letter present in the original word but in different index
//                 {
//                     FeedBack += "Y";
//                 }
//                 else //the letter not exits in the original word
//                 {
//                     FeedBack += "X";
//                 }
//             }

//             return FeedBack;
//         }
//     }
// }


using WordGuessing.Interfaces;

namespace WordGuessing.Services
{
    class FeedBackGenerator : IFeedBackGenerator
    {
        //method for giving feedback 
        public string GuessFeedback(string original, string guess)
        {
            char[] feedback = new char[5];

            bool[] used = new bool[5];

            //loop for checking the the letter match index as well
            for (int i = 0; i < 5; i++)
            {
                if (guess[i] == original[i]) //if the letter exists in the same index as the original.
                {
                    used[i] = true;
                    feedback[i] = 'G';
                }
                else
                {
                    feedback[i] = 'X';
                }
            }

            //next check the letter exists in a different syntax as well
            for (int i = 0; i < 5; i++)
            {
                if (feedback[i] != 'G') //check the field is not G
                {
                    for (int j = 0; j < 5; j++)
                    {
                        if (!used[j] && guess[i] == original[j]) //check the letter is not used
                        {
                            feedback[i] = 'Y';
                            used[j] = true;
                            break;
                        }
                    }
                }
            }

            return new string(feedback);
        }
    }
}