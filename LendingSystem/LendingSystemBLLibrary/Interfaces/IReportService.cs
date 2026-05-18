namespace LendingSystemBLLibrary.Interface
{
    public interface IReportService
    {
        List<object> GetCurrentlyBorrowedBooks(); //method for getting currently borrowed books

        List<object> GetOverdueBooks(); //method getting overdue books

        List<object> GetMembersWithPendingFines(); //method for getting books with pending fines

        List<object> GetMostBorrowedBooks(); //method for getting most borrowed books 

        List<object> GetAvailableBooksByCategory(int categoryid); //method for getting available books by category

        List<object> GetMemberBorrowingHistory(int memberid); //method for getting borrowing history by member id
        
        List<object> GetDamageReport();//method for getting all the damage reports
    }
}
