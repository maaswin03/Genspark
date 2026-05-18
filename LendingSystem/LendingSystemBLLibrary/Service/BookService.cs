using LendingSystemBLLibrary.Interface;
using LendingSystemDALLibrary.Interfaces;
using LendingSystemDALLibrary.Repository;
using LendingSystemModelLibrary.Exceptions;
using LendingSystemModelLibrary.Models;

namespace LendingSystemBLLibrary.Service
{
    public class BookService : IBookService
    {
        IBookRepository _bookrepo;
        IRepository<int, Bookcategory> _categoryrepo;

        //initializing book repo
        public BookService()
        {
            _bookrepo = new BookRepository();
            _categoryrepo = new BookCategoryRepository();
        }

        //method for creating a new book
        public Book? CreateBook(string title, string isbn, string author, int publishedyear, int categoryid)
        {
            Book book = new Book();
            book.Title = title;
            book.Author = author;
            book.Isbn = isbn;
            book.Publishedyear = publishedyear;
            book.Categoryid = categoryid;

            if (!ValidateBook(book))
            {
                return null;
            }

            return _bookrepo.Create(book);
        }

        //method for getting book by id
        public Book ViewBookById(int id)
        {
            var result = _bookrepo.GetById(id);

            if (result == null)
            {
                throw new InvalidInputException("OOPS, NO BOOK PRESENT WITH THIS ID!");
            }

            return result;
        }

        //method to view all books
        public List<Book> ViewAllBooks()
        {
            var result = _bookrepo.GetAll();

            if (result.Count == 0)
            {
                throw new InvalidInputException("OOPS, NO BOOK PRESENT IN THE COLLECTION!");
            }

            return result;
        }

        //method for getting book by title
        public List<Book> ViewBookByTitle(string title)
        {
            var result = _bookrepo.GetByTitle(title);

            if (result.Count == 0)
            {
                throw new InvalidInputException("OOPS, NO BOOK PRESENT WITH THIS TITLE!");
            }

            return result;
        }

        //method for getting book by author
        public List<Book> ViewBookByAuthor(string author)
        {
            var result = _bookrepo.GetByAuthor(author);

            if (result.Count == 0)
            {
                throw new InvalidInputException("OOPS, NO BOOK PRESENT IN THE COLLECTION!");
            }

            return result;
        }

        //method for getting book by category
        public List<Book> ViewBookByCategory(int id)
        {
            var result = _bookrepo.GetByCategory(id);

            if (result.Count == 0)
            {
                throw new InvalidInputException("OOPS, NO BOOK PRESENT WITH THIS CATEGORY!");
            }

            return result;
        }

        //method for validating book details
        public bool ValidateBook(Book book)
        {
            bool isbnExists = _bookrepo.GetAll().Any(b => b.Isbn == book.Isbn);
            bool categoryExist = _categoryrepo.GetById(book.Categoryid) != null;

            if (string.IsNullOrWhiteSpace(book.Title))
            {
                throw new InvalidInputException("PLEASE ENTER A VALID BOOK TITLE!");
            }
            else if (string.IsNullOrWhiteSpace(book.Author))
            {
                throw new InvalidInputException("PLEASE ENTER A VALID AUTHOR NAME!");
            }
            else if (book.Isbn.Length < 5)
            {
                throw new InvalidInputException("PLEASE ENTER A VALID ISBN!");
            }
            else if (book.Publishedyear < 1900 || book.Publishedyear > DateTime.Now.Year)
            {
                throw new InvalidInputException("PLEASE ENTER A VALID PUBLISHED YEAR!");
            }
            else if (isbnExists)
            {
                throw new InvalidInputException("OOPS, ISBN ALREADY EXISTS!");
            }
            else if (!categoryExist)
            {
                throw new InvalidInputException("OOPS, CATEGORY ID DOESN'T EXISTS!");
            }
            return true;
        }
    }
}