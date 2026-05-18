using LendingSystemDALLibrary.Contexts;
using LendingSystemDALLibrary.Interfaces;
using LendingSystemModelLibrary.Models;

namespace LendingSystemDALLibrary.Repository
{
    public class BorrowingRepository : IRepository<int, Borrowing>
    {
        BooklendingsystemContext _context;

        public BorrowingRepository()
        {
            _context = new BooklendingsystemContext();
        }

        //method for creating a new book Borrowing
        public Borrowing Create(Borrowing book)
        {
            _context.Borrowings.Add(book);
            _context.SaveChanges();
            return book;
        }

        //method for deleting a book Borrowing
        public Borrowing? Delete(int key)
        {
            var book = _context.Borrowings.FirstOrDefault(b => b.Id == key);

            if (book == null)
            {
                return null;
            }

            book.Borrowstatus = "RETURNED";
            book.Returndate = DateTime.Now;
            _context.SaveChanges();
            return book;
        }

        //method for updating book Borrowing
        public Borrowing? Update(Borrowing book)
        {
            var result = _context.Borrowings.FirstOrDefault(b => b.Id == book.Id);

            if (result == null)
            {
                return null;
            }

            _context.Borrowings.Update(book);
            _context.SaveChanges();
            return result;
        }

        //method for getting book Borrowings by id
        public Borrowing? GetById(int key)
        {
            return _context.Borrowings.FirstOrDefault(b => b.Id == key);
        }

        //method for getting all book Borrowing
        public List<Borrowing> GetAll()
        {
            return _context.Borrowings.ToList();
        }
    }
}