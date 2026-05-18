using LendingSystemModelLibrary.Models;

namespace LendingSystemBLLibrary.Interface
{
    public interface IBookService
    {
        //method for creating new books
        Book? CreateBook(string title, string isbn, string author, int publishedyear, int categoryid);

        //method for viewing books by id
        Book ViewBookById(int id);

        //method for viewing all books
        List<Book> ViewAllBooks();

        //method for viewing books by title
        List<Book> ViewBookByTitle(string title);

        //method for viewing books by category
        List<Book> ViewBookByCategory(int id);

        //method for viewing books by author
        List<Book> ViewBookByAuthor(string author);
    }
}
