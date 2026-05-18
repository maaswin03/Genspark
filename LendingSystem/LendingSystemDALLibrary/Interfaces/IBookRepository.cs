using LendingSystemModelLibrary.Models;

namespace LendingSystemDALLibrary.Interfaces
{
    public interface IBookRepository : IRepository<int, Book>
    {
        List<Book> GetByAuthor(string author); //method for getting book by author;

        List<Book> GetByTitle(string title); //method for getting book by title

        List<Book> GetByCategory(int id); //method for getting book by category
    }
}