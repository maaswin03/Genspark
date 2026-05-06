namespace WordGuessing.Interfaces
{
    //interface for feedback generator class
    interface IFeedBackGenerator
    {
        public string GuessFeedback(string original , string guess); //method for generating feedback;
    }
}