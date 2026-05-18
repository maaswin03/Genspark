using LendingSystemBLLibrary.Interface;
using LendingSystemDALLibrary.Interfaces;
using LendingSystemDALLibrary.Repository;
using LendingSystemModelLibrary.Exceptions;
using LendingSystemModelLibrary.Models;

namespace LendingSystemBLLibrary.Service
{
    public class BookCopyService : IBookCopyService
    {
        IRepository<int, Bookcopy> _bookcopyrepo;
        IRepository<int, Book> _bookrepo;

        public BookCopyService()
        {
            _bookcopyrepo = new BookCopyRepository();
            _bookrepo = new BookRepository();
        }

        //method for creating a new book copy
        public bool CreateBookCopies(int bookid, int noofcopies)
        {
            if (!ValidateBook(bookid, noofcopies))
            {
                return false;
            }
            for (int i = 0; i < noofcopies; i++)
            {
                Bookcopy bookcopy = new Bookcopy();
                bookcopy.Bookid = bookid;
                bookcopy.Bookstatus = "AVAILABLE";
                bookcopy.Createdat = DateTime.Now;

                _bookcopyrepo.Create(bookcopy);
            }
            return true;
        }

        //method for updating book status
        public Bookcopy? UpdateBookStatus(int bookcopyid, string status)
        {
            var result = _bookcopyrepo.GetById(bookcopyid);

            if (result == null)
            {
                throw new InvalidInputException("OOPS, BOOK COPY DOESN'T EXISTS!");
            }

            if (!ValidateStatus(status))
            {
                throw new InvalidInputException("PLEASE ENTER A VALID BOOK COPY STATUS!");
            }

            result.Bookstatus = status;
            return _bookcopyrepo.Update(result);
        }

        //method for getting all the book copies
        public List<Bookcopy> ViewAllBookCopies()
        {
            var result = _bookcopyrepo.GetAll();

            if (result.Count == 0)
            {
                throw new InvalidInputException("OOPS, NO BOOK COPY EXISTS!");
            }

            return result;
        }

        //method for getting book copy with id

        public Bookcopy? ViewBookCopyById(int id)
        {
            var result = _bookcopyrepo.GetById(id);

            if (result == null)
            {
                throw new InvalidInputException("OOPS, BOOK COPY DOESN'T EXISTS!");
            }

            return result;
        }

        //method for validation
        public bool ValidateBook(int bookid, int noofcopies)
        {
            bool BookExits = _bookrepo.GetById(bookid) != null;

            if (!BookExits)
            {
                throw new InvalidInputException("PLEASE ENTER A VALID BOOK ID!");
            }
            else if (noofcopies <= 0)
            {
                throw new InvalidInputException("PLEASE ENTER A VALID NO OF COPIES!");
            }
            return true;
        }

        public bool ValidateStatus(string status)
        {
            if (status != "AVAILABLE" && status != "BORROWED" && status != "DAMAGED")
            {
                return false;
            }
            return true;
        }
    }
}