using LibraryApi.Contexts;
using LibraryApi.Interfaces;
using LibraryApi.Models;

namespace LibraryApi.Repositories
{
    public class BookRepository : IBookRepository
    {
        protected LibraryDbContext _context;

        public BookRepository(LibraryDbContext context)
        {
            _context = context;
        }

        //method for inserting new book
        public Book Create(Book book)
        {
            _context.books.Add(book);
            _context.SaveChanges();
            return book;
        }

        //method for get all book details
        public List<Book> GetAll()
        {
            return _context.books.ToList();
        }

        //method for getting book by id
        public Book? GetById(int key)
        {
            return _context.books.FirstOrDefault(b => b.BookId == key);
        }

        //method for getting book by title
        public Book? GetByTitle(string key)
        {
            return _context.books.FirstOrDefault(b => b.Title == key);
        }
    }
}