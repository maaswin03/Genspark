using LendingSystemDALLibrary.Contexts;
using LendingSystemDALLibrary.Interfaces;
using LendingSystemModelLibrary.Models;

namespace LendingSystemDALLibrary.Repository
{
    public class BookRepository : IBookRepository
    {
        BooklendingsystemContext _context;

        //initializing context using constructor
        public BookRepository()
        {
            _context = new BooklendingsystemContext();
        }

        //method for creating new book
        public Book Create(Book book)
        {
            _context.Books.Add(book);
            _context.SaveChanges();
            return book;
        }

        //method for deleting a book
        public Book? Delete(int key)
        {
            var book = _context.Books.FirstOrDefault(b => b.Id == key);

            if (book == null)
            {
                return null;
            }

            _context.Books.Remove(book);
            _context.SaveChanges();
            return book;
        }

        //method for updating a book
        public Book? Update(Book book)
        {
            var result = _context.Books.FirstOrDefault(b => b.Id == book.Id);

            if (result == null)
            {
                return null;
            }

            _context.Books.Update(book);
            _context.SaveChanges();
            return result;
        }

        //method for getting book by id
        public Book? GetById(int key)
        {
            return _context.Books.FirstOrDefault(b => b.Id == key);
        }

        //method for getting all the Books
        public List<Book> GetAll()
        {
            return _context.Books.ToList();
        }

        //method for getting book by title
        public List<Book> GetByTitle(string title)
        {
            return _context.Books.Where(b => b.Title == title).ToList();
        }

        //method for getting books by author
        public List<Book> GetByAuthor(string author)
        {
            return _context.Books.Where(b => b.Author == author).ToList();
        }

        //method for getting books by category
        public List<Book> GetByCategory(int id)
        {
            return _context.Books.Where(b => b.Categoryid == id).ToList();
        }
    }
}