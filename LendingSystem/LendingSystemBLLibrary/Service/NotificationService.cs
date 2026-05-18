using LendingSystemBLLibrary.Interface;
using LendingSystemBLLibrary.Sender;
using LendingSystemDALLibrary.Contexts;
using LendingSystemModelLibrary.Exceptions;

namespace LendingSystemBLLibrary.Service
{
    public class NotificationService : INotificationService
    {
        BooklendingsystemContext _context;
        NotificationSender sender;

        public NotificationService()
        {
            _context = new BooklendingsystemContext();
            sender = new NotificationSender();
        }

        //method for sending notification for overdue
        public void OverDueNotification()
        {
            var result = (from br in _context.Borrowings join m in _context.Members on br.Memberid equals m.Id join bc in _context.Bookcopies on br.Bookcopyid equals bc.Id join b in _context.Books on bc.Bookid equals b.Id where br.Borrowstatus == "BORROWED" && br.Duedate < DateTime.Now select new { MemberName = m.Name, Email = m.Email, Title = b.Title }).ToList();

            if (result.Count == 0)
            {
                throw new InvalidInputException("OOPS, NO OVERDUE BOOKS PRESENT !");
            }

            foreach (var r in result)
            {
                sender.OverDueNotification(r.MemberName, r.Email, r.Title);
            }
        }

        //method for sending notification for overdue
        public void FineNotification()
        {
            var result = (from f in _context.Fines join m in _context.Members on f.Memberid equals m.Id where f.Ispaid == false select new { MemberName = m.Name, Email = m.Email, FineAmount = f.Amount, }).ToList();

            if (result.Count == 0)
            {
                throw new InvalidInputException("OOPS, NO PENDING FINES PRESENT !");
            }

            foreach (var r in result)
            {
                sender.FineNotification(r.MemberName, r.Email, r.FineAmount);
            }
        }
    }
}