using LibraryApi.Models;

namespace LibraryApi.Interfaces
{
    public interface IBookRepository : IRepository<int, Book>
    {
        Book? GetByTitle(string key); //method for getting book by title
    }
}