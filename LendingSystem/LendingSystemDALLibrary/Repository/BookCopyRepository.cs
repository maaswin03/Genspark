using LendingSystemDALLibrary.Contexts;
using LendingSystemModelLibrary.Models;
using LendingSystemDALLibrary.Interfaces;

namespace LendingSystemDALLibrary.Repository
{
    public class BookCopyRepository : IRepository<int, Bookcopy>
    {
        BooklendingsystemContext _context;

        public BookCopyRepository()
        {
            _context = new BooklendingsystemContext();
        }

        //method for creating a new book copy
        public Bookcopy Create(Bookcopy book)
        {
            _context.Bookcopies.Add(book);
            _context.SaveChanges();
            return book;
        }

        //method for deleting a book copy
        public Bookcopy? Delete(int key)
        {
            var bookcopy = _context.Bookcopies.FirstOrDefault(b => b.Id == key);

            if (bookcopy == null)
            {
                return null;
            }

            bookcopy.Bookstatus = "DAMAGED";
            _context.SaveChanges();
            return bookcopy;
        }

        //method for updating bookcopy
        public Bookcopy? Update(Bookcopy book)
        {
            var result = _context.Bookcopies.FirstOrDefault(b => b.Id == book.Id);

            if (result == null)
            {
                return null;
            }

            result.Bookstatus = book.Bookstatus;
            _context.SaveChanges();
            return result;
        }

        //method for getting bookcopies by id
        public Bookcopy? GetById(int key)
        {
            return _context.Bookcopies.FirstOrDefault(b => b.Id == key);
        }

        //method for getting all bookcopies
        public List<Bookcopy> GetAll()
        {
            return _context.Bookcopies.ToList();
        }
    }
}