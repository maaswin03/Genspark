namespace LendingSystemBLLibrary.Sender
{
    public class NotificationSender
    {
        public void OverDueNotification(string Name, string Email, string Title)
        {
            Console.WriteLine($"\nNOTIFICATION SUCCESSFULLY SENDED TO {Name} VIA {Email} FOR NOT RETURNING {Title} BOOK");
        }

        public void FineNotification(string Name, string Email, decimal Amount)
        {
            Console.WriteLine($"\nNOTIFICATION SUCCESSFULLY SENDED TO {Name} VIA {Email} FOR PENDING FINE ₹{Amount}");
        }
    }
}