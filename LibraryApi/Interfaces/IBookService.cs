using LibraryApi.Models;

namespace LibraryApi.Interfaces
{
    public interface IBookService
    {
        Book? CreateBook(Book book); //method for creating new book

        List<Book> GetAllBooks(); //method for getting all the books

        Book? GetBookById(int id); //method for getting book by id

        Book? GetBookByTitle(string title); //method for getting book by title
    }
}