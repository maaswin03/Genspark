namespace LendingSystemBLLibrary.Interface
{
    public interface INotificationService
    {
        void OverDueNotification(); //method for sending overdue borrow notification
        void FineNotification(); //method for sending pending fine notification 
    }
}