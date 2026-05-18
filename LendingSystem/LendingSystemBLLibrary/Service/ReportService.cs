using LendingSystemBLLibrary.Interface;
using LendingSystemDALLibrary.Contexts;
using LendingSystemModelLibrary.Models;

namespace LendingSystemBLLibrary.Service
{
    public class ReportService : IReportService
    {
        BooklendingsystemContext _context;

        public ReportService()
        {
            _context = new BooklendingsystemContext();
        }

        //method for getting currently borrowed books
        public List<object> GetCurrentlyBorrowedBooks()
        {
            var result = (from br in _context.Borrowings join bc in _context.Bookcopies on br.Bookcopyid equals bc.Id join b in _context.Books on bc.Bookid equals b.Id where br.Borrowstatus == "BORROWED" select new { BorrowingId = br.Id, BookTitle = b.Title, BorrowDate = br.Borrowdate, DueDate = br.Duedate }).ToList<object>();
            return result;
        }

        //method for getting currently borrowed books
        public List<object> GetOverdueBooks()
        {
            var result = (from br in _context.Borrowings join bc in _context.Bookcopies on br.Bookcopyid equals bc.Id join b in _context.Books on bc.Bookid equals b.Id where br.Borrowstatus == "BORROWED" && br.Duedate < DateTime.Now select new { BorrowingId = br.Id, BookTitle = b.Title, DueDate = br.Duedate }).ToList<object>();
            return result;
        }

        //method for getting books with pending fines
        public List<object> GetMembersWithPendingFines()
        {
            var result = (from f in _context.Fines join m in _context.Members on f.Memberid equals m.Id where f.Ispaid == false select new { MemberId = m.Id, MemberName = m.Name, FineAmount = f.Amount, Reason = f.Finereason }).ToList<object>();
            return result;
        }

        //method for getting most borrowed books 
        public List<object> GetMostBorrowedBooks()
        {
            var result = (from br in _context.Borrowings join bc in _context.Bookcopies on br.Bookcopyid equals bc.Id join b in _context.Books on bc.Bookid equals b.Id group br by b.Title into g orderby g.Count() descending select new { BookTitle = g.Key, BorrowCount = g.Count() }).ToList<object>();
            return result;
        }

        //method for getting available books by category
        public List<object> GetAvailableBooksByCategory(int categoryid)
        {
            var result = (from bc in _context.Bookcopies join b in _context.Books on bc.Bookid equals b.Id join c in _context.Bookcategories on b.Categoryid equals c.Id where bc.Bookstatus == "AVAILABLE" && c.Id == categoryid select new { BookId = b.Id, BookTitle = b.Title, Category = c.Categoryname }).ToList<object>();
            return result;
        }

        //method for getting borrowing history by member id
        public List<object> GetMemberBorrowingHistory(int memberid)
        {
            var result = (from br in _context.Borrowings join bc in _context.Bookcopies on br.Bookcopyid equals bc.Id join b in _context.Books on bc.Bookid equals b.Id where br.Memberid == memberid select new { BorrowingId = br.Id, BookTitle = b.Title, BorrowDate = br.Borrowdate, ReturnDate = br.Returndate, Status = br.Borrowstatus }).ToList<object>();
            return result;
        }

        //method for getting all the damage reports
        public List<object> GetDamageReport()
        {
            var result = (from d in _context.Bookdamagereports join br in _context.Borrowings on d.Borrowingid equals br.Id join m in _context.Members on br.Memberid equals m.Id where d.Isresolved == false select new { DamageId = d.Id, Description = d.Description, ReportedAt = d.Reportedat, BookCopyId = br.Bookcopyid, BorrowingId = br.Id, MemberName = m.Name, MemberEmail = m.Email }).ToList<object>();
            return result;
        }
    }
}
