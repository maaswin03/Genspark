namespace LendingSystemBLLibrary.Interface
{
    public interface IBorrowService
    {
        bool BorrowBook(int memberid, int bookcopyid);//method for creating borrow transaction

        bool ReturnBook(int borrowingid, string status);//method for creating return transaction
    }
}