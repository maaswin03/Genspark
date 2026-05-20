using LibraryApi.Interfaces;
using LibraryApi.Misc;
using LibraryApi.Models;

namespace LibraryApi.Services
{
    public class BookService : IBookService
    {
        IBookRepository _bookrepo;

        public BookService(IBookRepository repository)
        {
            _bookrepo = repository;
        }

        //method for creating a new book
        public Book? CreateBook(Book book)
        {
            if (!ValidateBook(book))
            {
                return null;
            }
            book.Title = book.Title.ToUpper();
            book.Author = book.Author.ToUpper();
            var result = _bookrepo.Create(book);
            return result;
        }

        //method for getting all the book details
        public List<Book> GetAllBooks()
        {
            return _bookrepo.GetAll();
        }

        //method for getting books by id
        public Book? GetBookById(int id)
        {
            return _bookrepo.GetById(id);
        }

        //method for getting book by title
        public Book? GetBookByTitle(string title)
        {
            return _bookrepo.GetByTitle(title.ToUpper());
        }

        //method for validating the book
        public bool ValidateBook(Book book)
        {
            bool isbnExists = _bookrepo.GetAll().Any(b => b.Isbn == book.Isbn);
            bool BookidExists = _bookrepo.GetById(book.BookId) != null;

            if (BookidExists)
            {
                throw new InvalidInputException("OOPS, BOOK ID ALREADY EXISTS");
            }
            else if (string.IsNullOrWhiteSpace(book.Title))
            {
                throw new InvalidInputException("PLEASE ENTER A VALID TITLE");
            }
            else if (string.IsNullOrWhiteSpace(book.Author))
            {
                throw new InvalidInputException("PLEASE ENTER A VALID AUTHOR NAME");
            }
            else if (string.IsNullOrWhiteSpace(book.Isbn))
            {
                throw new InvalidInputException("PLEASE ENTER A ISBN");
            }
            else if (book.PublishedYear < 1900 || book.PublishedYear > DateTime.Now.Year)
            {
                throw new InvalidInputException("PLEASE ENTER A VALID PUBLISHED YEAR");
            }
            else if (book.AvailableCopies < 0)
            {
                throw new InvalidInputException("PLEASE ENTER A VALID AVAILABLE COPIES");
            }
            else if (isbnExists)
            {
                throw new InvalidInputException("OOPS, ISBN ALREADY EXISTS FOR OTHER BOOK");
            }
            return true;
        }
    }
}