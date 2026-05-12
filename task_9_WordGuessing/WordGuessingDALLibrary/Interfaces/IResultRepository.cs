using WordGuessingModelLibrary;

namespace WordGuessingDALLibrary.Repository
{
    public interface IResultRepository
    {
        public Results? Create(Results result); //method for creating a new result

        public List<Leader>? GetData(); //method for fetching all the results

        public List<Results>? GetUserData(string username); //get data for a particular user
    }
}