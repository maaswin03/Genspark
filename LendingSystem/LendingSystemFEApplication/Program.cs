using LendingSystemBLLibrary.Interface;
using LendingSystemBLLibrary.Service;
using LendingSystemModelLibrary.Exceptions;

namespace LendingSystemFEApplication
{
    public class Program
    {
        static IMemberService member = new MemberService();
        static IBookService book = new BookService();
        static IBookCopyService bookcopy = new BookCopyService();
        static IBorrowService borrow = new BorrowingService();
        static IFineService fine = new FineService();
        static IReportService report = new ReportService();
        static IOtherService other = new OtherService();
        static INotificationService notification = new NotificationService();

        //method to manage members
        public void MemberManagement()
        {
            while (true)
            {
                try
                {
                    Console.WriteLine("\n----------------------------------------------------------");
                    Console.WriteLine("\nBELOW ARE THE CONSOLE MENUS");
                    Console.WriteLine("ENTER 1 FOR ADDING A NEW MEMBER");
                    Console.WriteLine("ENTER 2 FOR VIEWING ALL MEMBERS");
                    Console.WriteLine("ENTER 3 FOR SEARCHING MEMBER BY EMAIL");
                    Console.WriteLine("ENTER 4 FOR SEARCHING MEMBER BY PHONE");
                    Console.WriteLine("ENTER 5 FOR UPDATING MEMBERSHIP STATUS");
                    Console.WriteLine("ENTER 6 FOR DEACTIVATING A MEMBER");
                    Console.WriteLine("ENTER 7 FOR EXIT");
                    Console.WriteLine("\n----------------------------------------------------------");

                    Console.WriteLine("\nPLEASE ENTER THE OPTION : ");
                    int n = 0;
                    while (!int.TryParse(Console.ReadLine(), out n) || (n <= 0 || n > 7))
                    {
                        Console.WriteLine("\nPLEASE ENTER A VALID OPTION");
                    }

                    if (n == 1) //condition for creating a new member
                    {
                        Console.WriteLine("\nPLEASE ENTER MEMBER NAME : ");
                        string name = Console.ReadLine() ?? "";

                        Console.WriteLine("\nPLEASE ENTER MEMBER EMAIL : ");
                        string email = Console.ReadLine() ?? "";

                        Console.WriteLine("\nPLEASE ENTER MEMBER PHONE NUMBER : ");
                        string phone = Console.ReadLine() ?? "";


                        Console.WriteLine("\nBELOW ARE THE MEMBERSHIP TYPE");
                        var memresult = other.GetAllMembership();

                        foreach (var r in memresult)
                        {
                            Console.WriteLine($"{r.Id} - {r.Typename}");
                        }

                        Console.WriteLine("\nPLEASE ENTER MEMBERSHIP ID : ");
                        int membershiptypeid = 0;
                        while (!int.TryParse(Console.ReadLine(), out membershiptypeid) || (membershiptypeid <= 0))
                        {
                            Console.WriteLine("\nPLEASE ENTER A VALID MEMBERSHIP ID");
                        }

                        var result = member.CreateMember(name.ToUpper(), email.ToLower(), phone, membershiptypeid);

                        if (result != null)
                        {
                            Console.WriteLine("\nMEMBER CREATED SUCCESSFULLY!");
                        }
                        else
                        {
                            Console.WriteLine("\nMEMBER CREATION FAILED! PLEASE TRY AGAIN LATER");
                        }
                    }
                    else if (n == 2) //condition for viewing all members
                    {
                        var result = member.ViewAllMember();

                        Console.WriteLine("\nBELOW IS THE MEMBER LIST");

                        foreach (var r in result)
                        {
                            Console.WriteLine($"\nID : {r.Id} | NAME : {r.Name} | EMAIL : {r.Email} | PHONE : +91-{r.Phone} | STATUS : {(r.Isactive ? "ACTIVE" : "INACTIVE")}");
                        }
                    }
                    else if (n == 3) //condition for getting member by email
                    {
                        Console.WriteLine("\nPLEASE ENTER MEMBER EMAIL : ");
                        string email = Console.ReadLine() ?? "";

                        var r = member.ViewMemberByEmail(email.ToLower());
                        Console.WriteLine($"\nID : {r.Id} | NAME : {r.Name} | EMAIL : {r.Email} | PHONE : +91-{r.Phone} | STATUS : {(r.Isactive ? "ACTIVE" : "INACTIVE")}");
                    }
                    else if (n == 4) //condition for getting member by phone
                    {
                        Console.WriteLine("\nPLEASE ENTER MEMBER PHONE NUMBER : ");
                        string phone = Console.ReadLine() ?? "";

                        var r = member.ViewMemberByPhone(phone);
                        Console.WriteLine($"\nID : {r.Id} | NAME : {r.Name} | EMAIL : {r.Email} | PHONE : +91-{r.Phone} | STATUS : {(r.Isactive ? "ACTIVE" : "INACTIVE")}");
                    }
                    else if (n == 5) //condition for updating the membership status
                    {
                        Console.WriteLine("\nPLEASE ENTER MEMBER ID: ");
                        int id = 0;
                        while (!int.TryParse(Console.ReadLine(), out id) || (id <= 0))
                        {
                            Console.WriteLine("\nPLEASE ENTER A VALID MEMBER ID");
                        }

                        Console.WriteLine("\nBELOW ARE THE MEMBERSHIP TYPE");
                        var memresult = other.GetAllMembership();

                        foreach (var r in memresult)
                        {
                            Console.WriteLine($"{r.Id} - {r.Typename}");
                        }

                        Console.WriteLine("\nPLEASE ENTER MEMBERSHIP ID : ");
                        int membershiptypeid = 0;
                        while (!int.TryParse(Console.ReadLine(), out membershiptypeid) || (membershiptypeid <= 0))
                        {
                            Console.WriteLine("\nPLEASE ENTER A VALID MEMBERSHIP ID");
                        }

                        var result = member.UpdateMemberShipStatus(id, membershiptypeid);

                        if (result != null)
                        {
                            Console.WriteLine("\nMEMBERSHIP STATUS UPDATED SUCCESSFULLY");
                        }
                        else
                        {
                            Console.WriteLine("\nMEMBERSHIP STATUS UPDATE FAILED! TRY AGAIN LATER");
                        }
                    }
                    else if (n == 6) //condition for deactivating member
                    {
                        Console.WriteLine("\nPLEASE ENTER MEMBER ID: ");
                        int id = 0;
                        while (!int.TryParse(Console.ReadLine(), out id) || (id <= 0))
                        {
                            Console.WriteLine("\nPLEASE ENTER A VALID MEMBER ID");
                        }

                        var result = member.DeactivateMember(id);

                        if (result != null)
                        {
                            Console.WriteLine("\nMEMBER DEACTIVATED SUCCESSFULLY");
                        }
                        else
                        {
                            Console.WriteLine("\nMEMBER DEACTIVATION FAILED! TRY AGAIN LATER");
                        }
                    }
                    else if (n == 7)
                    {
                        break;
                    }
                }
                catch (InvalidInputException e)
                {
                    Console.WriteLine($"\n{e.Message}");
                }
            }
        }

        //method for managing books
        public void BookManagement()
        {
            while (true)
            {
                try
                {
                    Console.WriteLine("\n----------------------------------------------------------");
                    Console.WriteLine("\nBELOW ARE THE CONSOLE MENUS");
                    Console.WriteLine("ENTER 1 FOR ADDING A NEW BOOK");
                    Console.WriteLine("ENTER 2 FOR ADDING MULTIPLE COPIES OF THE SAME BOOK");
                    Console.WriteLine("ENTER 3 FOR VIEWING ALL THE BOOKS");
                    Console.WriteLine("ENTER 4 FOR SEARCHING BOOK BY TITLE");
                    Console.WriteLine("ENTER 5 FOR SEARCHING BOOK BY AUTHOR");
                    Console.WriteLine("ENTER 6 FOR SEARCHING BOOK BY CATEGORY");
                    Console.WriteLine("ENTER 7 FOR UPDATING BOOK COPY STATUS");
                    Console.WriteLine("ENTER 8 FOR ADDING A NEW CATEGORY");
                    Console.WriteLine("ENTER 9 FOR VIEWING ALL CATEGORIES");
                    Console.WriteLine("ENTER 10 FOR EXIT");
                    Console.WriteLine("\n----------------------------------------------------------");

                    Console.WriteLine("\nPLEASE ENTER THE OPTION : ");
                    int n = 0;
                    while (!int.TryParse(Console.ReadLine(), out n) || (n <= 0 || n > 10))
                    {
                        Console.WriteLine("\nPLEASE ENTER A VALID OPTION");
                    }

                    if (n == 1) //condition for adding a new book
                    {
                        Console.WriteLine("\nPLEASE ENTER BOOK TITLE : ");
                        string title = Console.ReadLine() ?? "";

                        Console.WriteLine("\nPLEASE ENTER ISBN : ");
                        string isbn = Console.ReadLine() ?? "";

                        Console.WriteLine("\nPLEASE ENTER AUTHOR NAME : ");
                        string author = Console.ReadLine() ?? "";

                        Console.WriteLine("\nPLEASE ENTER PUBLISHED YEAR : ");
                        int publishedyear = 0;
                        while (!int.TryParse(Console.ReadLine(), out publishedyear) || publishedyear < 1900 || publishedyear > DateTime.Now.Year)
                        {
                            Console.WriteLine("\nPLEASE ENTER A VALID PUBLISHED YEAR");
                        }

                        Console.WriteLine("\nBELOW ARE THE CATEGORY TYPE ");
                        var memresult = other.GetAllCategory();

                        foreach (var r in memresult)
                        {
                            Console.WriteLine($"{r.Id} - {r.Categoryname}");
                        }

                        Console.WriteLine("\nPLEASE ENTER CATEGORY ID : ");
                        int categoryid = 0;
                        while (!int.TryParse(Console.ReadLine(), out categoryid) || categoryid <= 0)
                        {
                            Console.WriteLine("\nPLEASE ENTER A VALID CATEGORY ID");
                        }

                        var result = book.CreateBook(title.ToUpper(), isbn.ToUpper(), author.ToUpper(), publishedyear, categoryid);

                        if (result != null)
                        {
                            Console.WriteLine("\nBOOK CREATED SUCCESSFULLY!");
                        }
                        else
                        {
                            Console.WriteLine("\nBOOK CREATION FAILED!");
                        }
                    }
                    else if (n == 2) //condition for adding multiple book copies
                    {
                        Console.WriteLine("\nPLEASE ENTER BOOK ID : ");
                        int bookid = 0;
                        while (!int.TryParse(Console.ReadLine(), out bookid) || bookid <= 0)
                        {
                            Console.WriteLine("\nPLEASE ENTER A VALID BOOK ID");
                        }

                        Console.WriteLine("\nPLEASE ENTER NUMBER OF COPIES : ");
                        int copies = 0;
                        while (!int.TryParse(Console.ReadLine(), out copies) || copies <= 0)
                        {
                            Console.WriteLine("\nPLEASE ENTER A VALID NUMBER OF COPIES");
                        }

                        bool result = bookcopy.CreateBookCopies(bookid, copies);

                        if (result)
                        {
                            Console.WriteLine("\nBOOK COPIES ADDED SUCCESSFULLY!");
                        }
                        else
                        {
                            Console.WriteLine("\nFAILED TO ADD BOOK COPIES!");
                        }
                    }
                    else if (n == 3) //condition for viewing all books
                    {
                        var result = book.ViewAllBooks();

                        Console.WriteLine("\nBELOW ARE THE BOOK DETAILS");

                        foreach (var r in result)
                        {
                            Console.WriteLine($"\n ID : {r.Id} | TITLE : {r.Title} | AUTHOR : {r.Author} | ISBN : {r.Isbn}");
                        }
                    }
                    else if (n == 4) //condition for searching book by title
                    {
                        Console.WriteLine("\nPLEASE ENTER BOOK TITLE : ");
                        string title = Console.ReadLine() ?? "";

                        var result = book.ViewBookByTitle(title.ToUpper());

                        foreach (var r in result)
                        {
                            Console.WriteLine($"\n ID : {r.Id} | TITLE : {r.Title} | AUTHOR : {r.Author} | ISBN : {r.Isbn}");
                        }
                    }
                    else if (n == 5) //condition for searching book by author
                    {
                        Console.WriteLine("\nPLEASE ENTER AUTHOR NAME : ");
                        string author = Console.ReadLine() ?? "";

                        var result = book.ViewBookByAuthor(author.ToUpper());

                        foreach (var r in result)
                        {
                            Console.WriteLine($"\n ID : {r.Id} | TITLE : {r.Title} | AUTHOR : {r.Author} | ISBN : {r.Isbn}");
                        }
                    }
                    else if (n == 6) //condition for searching book by category
                    {
                        Console.WriteLine("\nBELOW ARE THE CATEGORY TYPE ");
                        var memresult = other.GetAllCategory();

                        foreach (var r in memresult)
                        {
                            Console.WriteLine($"ID : {r.Id} | NAME : {r.Categoryname}");
                        }

                        Console.WriteLine("\nPLEASE ENTER CATEGORY ID : ");
                        int categoryid = 0;
                        while (!int.TryParse(Console.ReadLine(), out categoryid) || categoryid <= 0)
                        {
                            Console.WriteLine("\nPLEASE ENTER A VALID CATEGORY ID");
                        }

                        var result = book.ViewBookByCategory(categoryid);

                        foreach (var r in result)
                        {
                            Console.WriteLine($"\n ID : {r.Id} | TITLE : {r.Title} | AUTHOR : {r.Author} | ISBN : {r.Isbn}");
                        }
                    }
                    else if (n == 7) //condition for updating book copy status
                    {
                        Console.WriteLine("\nPLEASE ENTER BOOK COPY ID : ");
                        int copyid = 0;
                        while (!int.TryParse(Console.ReadLine(), out copyid) || copyid <= 0)
                        {
                            Console.WriteLine("\nPLEASE ENTER A VALID BOOK COPY ID");
                        }

                        Console.WriteLine("\nPLEASE ENTER STATUS (AVAILABLE / BORROWED / DAMAGED) : ");
                        string status = Console.ReadLine() ?? "";

                        var result = bookcopy.UpdateBookStatus(copyid, status.ToUpper());

                        if (result != null)
                        {
                            Console.WriteLine("\nBOOK COPY STATUS UPDATED SUCCESSFULLY!");
                        }
                        else
                        {
                            Console.WriteLine("\nFAILED TO UPDATE BOOK COPY STATUS!");
                        }
                    }
                    else if (n == 8) //condition for adding a new category
                    {
                        Console.WriteLine("\nPLEASE ENTER CATEGORY NAME : ");
                        string name = Console.ReadLine() ?? "";

                        var result = other.CreateCategory(name);

                        if (result != null)
                        {
                            Console.WriteLine("\nCATEGORY CREATED SUCCESSFULLY!");
                        }
                        else
                        {
                            Console.WriteLine("\nFAILED TO CREATE CATEGORY!");
                        }
                    }
                    else if (n == 9) //condition for viewing all categories
                    {
                        var result = other.GetAllCategory();

                        Console.WriteLine("\nBELOW ARE THE CATEGORIES");

                        foreach (var c in result)
                        {
                            Console.WriteLine($"\nID : {c.Id} | NAME : {c.Categoryname}");
                        }
                    }
                    else if (n == 10)
                    {
                        break;
                    }
                }
                catch (InvalidInputException e)
                {
                    Console.WriteLine($"\n{e.Message}");
                }
            }
        }

        //method for managing borrow
        public void BorrowManagement()
        {
            while (true)
            {
                try
                {
                    Console.WriteLine("\n----------------------------------------------------------");
                    Console.WriteLine("\nBELOW ARE THE CONSOLE MENUS");
                    Console.WriteLine("ENTER 1 FOR BORROWING A BOOK");
                    Console.WriteLine("ENTER 2 FOR EXIT");
                    Console.WriteLine("\n----------------------------------------------------------");

                    Console.WriteLine("\nPLEASE ENTER THE OPTION : ");
                    int n = 0;
                    while (!int.TryParse(Console.ReadLine(), out n) || (n <= 0 || n > 2))
                    {
                        Console.WriteLine("\nPLEASE ENTER A VALID OPTION");
                    }

                    if (n == 1) //condition for borrowing a book
                    {
                        Console.WriteLine("\nPLEASE ENTER MEMBER ID : ");
                        int memberid = 0;
                        while (!int.TryParse(Console.ReadLine(), out memberid) || memberid <= 0)
                        {
                            Console.WriteLine("\nPLEASE ENTER A VALID MEMBER ID");
                        }

                        Console.WriteLine("\nPLEASE ENTER BOOK COPY ID : ");
                        int bookcopyid = 0;
                        while (!int.TryParse(Console.ReadLine(), out bookcopyid) || bookcopyid <= 0)
                        {
                            Console.WriteLine("\nPLEASE ENTER A VALID BOOK COPY ID");
                        }

                        bool result = borrow.BorrowBook(memberid, bookcopyid);

                        if (result)
                        {
                            Console.WriteLine("\nBOOK BORROWED SUCCESSFULLY!");
                        }
                        else
                        {
                            Console.WriteLine("\nFAILED TO BORROW BOOK! PLEASE CHECK THE DETAILS AND TRY AGAIN");
                        }
                    }
                    else if (n == 2)
                    {
                        break;
                    }
                }
                catch (InvalidInputException e)
                {
                    Console.WriteLine($"\n{e.Message}");
                }
            }
        }

        //method for managing return
        public void ReturnManagement()
        {
            while (true)
            {
                try
                {
                    Console.WriteLine("\n----------------------------------------------------------");
                    Console.WriteLine("\nBELOW ARE THE CONSOLE MENUS");
                    Console.WriteLine("ENTER 1 FOR RETURNING A BOOK");
                    Console.WriteLine("ENTER 2 FOR VIEWING THE BORROWING HISTORY");
                    Console.WriteLine("ENTER 3 FOR EXIT");
                    Console.WriteLine("\n----------------------------------------------------------");

                    Console.WriteLine("\nPLEASE ENTER THE OPTION : ");
                    int n = 0;
                    while (!int.TryParse(Console.ReadLine(), out n) || (n <= 0 || n > 3))
                    {
                        Console.WriteLine("\nPLEASE ENTER A VALID OPTION");
                    }
                    if (n == 1) //condition for returning a book
                    {
                        Console.WriteLine("\nPLEASE ENTER BORROWING ID : ");
                        int borrowingid = 0;
                        while (!int.TryParse(Console.ReadLine(), out borrowingid) || borrowingid <= 0)
                        {
                            Console.WriteLine("\nPLEASE ENTER A VALID BORROWING ID");
                        }

                        Console.WriteLine("\nPLEASE ENTER BOOK CONDITION (NORMAL / DAMAGED / LOST) : ");
                        string condition = Console.ReadLine() ?? "";

                        bool result = borrow.ReturnBook(borrowingid, condition.ToUpper());

                        if (result)
                        {
                            Console.WriteLine("\nBOOK RETURNED SUCCESSFULLY!");
                        }
                        else
                        {
                            Console.WriteLine("\nFAILED TO RETURN BOOK! PLEASE CHECK THE DETAILS AND TRY AGAIN");
                        }
                    }
                    else if (n == 2) //condition for showing all the borrowings
                    {
                        var result = report.GetCurrentlyBorrowedBooks();

                        if (result.Count == 0)
                        {
                            Console.WriteLine("\nNO BOOKS CURRENTLY BORROWED");
                        }
                        else
                        {
                            Console.WriteLine("\nBELOW ARE THE CURRENTLY BORROWED BOOKS");

                            foreach (var r in result)
                            {
                                Console.WriteLine($"\n{r}");
                            }
                        }
                    }
                    else if (n == 3)
                    {
                        break;
                    }
                }
                catch (InvalidInputException e)
                {
                    Console.WriteLine($"\n{e.Message}");
                }
            }
        }

        //method for managing fines
        public void FineManagement()
        {
            while (true)
            {
                try
                {
                    Console.WriteLine("\n----------------------------------------------------------");
                    Console.WriteLine("\nBELOW ARE THE CONSOLE MENUS");
                    Console.WriteLine("ENTER 1 FOR VIEWING PENDING FINES OF A MEMBER");
                    Console.WriteLine("ENTER 2 FOR VIEWING FINE HISTORY OF A MEMBER");
                    Console.WriteLine("ENTER 3 FOR PAYING A FINE");
                    Console.WriteLine("ENTER 4 FOR EXIT");
                    Console.WriteLine("\n----------------------------------------------------------");

                    Console.WriteLine("\nPLEASE ENTER THE OPTION : ");
                    int n = 0;
                    while (!int.TryParse(Console.ReadLine(), out n) || (n <= 0 || n > 4))
                    {
                        Console.WriteLine("\nPLEASE ENTER A VALID OPTION");
                    }

                    if (n == 1) //condition for viewing pending fines
                    {
                        Console.WriteLine("\nPLEASE ENTER MEMBER ID : ");
                        int memberid = 0;
                        while (!int.TryParse(Console.ReadLine(), out memberid) || memberid <= 0)
                        {
                            Console.WriteLine("\nPLEASE ENTER A VALID MEMBER ID");
                        }

                        var result = fine.GetAllFineById(memberid);

                        Console.WriteLine("\nBELOW ARE THE PENDING FINES");

                        foreach (var f in result)
                        {
                            Console.WriteLine($"\nFINE ID : {f.Id} | AMOUNT : ₹{f.Amount} | REASON : {f.Finereason} | CREATED : {f.Createdat:dd-MM-yyyy}");
                        }
                    }
                    else if (n == 2) //condition for viewing fine history
                    {
                        Console.WriteLine("\nPLEASE ENTER MEMBER ID : ");
                        int memberid = 0;
                        while (!int.TryParse(Console.ReadLine(), out memberid) || memberid <= 0)
                        {
                            Console.WriteLine("\nPLEASE ENTER A VALID MEMBER ID");
                        }

                        var result = fine.GetAllFine(memberid);

                        Console.WriteLine("\nBELOW IS THE FINE HISTORY");

                        foreach (var f in result)
                        {
                            Console.WriteLine($"\nFINE ID : {f.Id} | AMOUNT : ₹{f.Amount} | REASON : {f.Finereason} | PAID : {(f.Ispaid ? "YES" : "NO")} | CREATED : {f.Createdat:dd-MM-yyyy}");
                        }
                    }
                    else if (n == 3) //condition for paying fine
                    {
                        Console.WriteLine("\nPLEASE ENTER FINE ID : ");
                        int fineid = 0;
                        while (!int.TryParse(Console.ReadLine(), out fineid) || fineid <= 0)
                        {
                            Console.WriteLine("\nPLEASE ENTER A VALID FINE ID");
                        }

                        Console.WriteLine("\nPLEASE ENTER PAYMENT METHOD (CARD / UPI / NET_BANKING / WALLET) : ");
                        string method = Console.ReadLine() ?? "";

                        var result = fine.PayFine(fineid, method.ToUpper());

                        if (result != null)
                        {
                            Console.WriteLine($"\nFINE PAID SUCCESSFULLY! AMOUNT PAID : ₹{result.Amountpaid}");
                        }
                        else
                        {
                            Console.WriteLine("\nFAILED TO PAY FINE! PLEASE TRY AGAIN");
                        }
                    }
                    else if (n == 4)
                    {
                        break;
                    }
                }
                catch (InvalidInputException e)
                {
                    Console.WriteLine($"\n{e.Message}");
                }
            }
        }

        //method for managing reports
        public void ReportManagement()
        {
            while (true)
            {
                try
                {
                    Console.WriteLine("\n----------------------------------------------------------");
                    Console.WriteLine("\nBELOW ARE THE CONSOLE MENUS");
                    Console.WriteLine("ENTER 1 FOR BOOKS CURRENTLY BORROWED");
                    Console.WriteLine("ENTER 2 FOR OVERDUE BOOKS");
                    Console.WriteLine("ENTER 3 FOR MEMBERS WITH PENDING FINES");
                    Console.WriteLine("ENTER 4 FOR MOST BORROWED BOOKS");
                    Console.WriteLine("ENTER 5 FOR AVAILABLE BOOKS BY CATEGORY");
                    Console.WriteLine("ENTER 6 FOR MEMBER BORROWING HISTORY");
                    Console.WriteLine("ENTER 7 FOR MEMBER DAMAGE REPORT");
                    Console.WriteLine("ENTER 8 FOR EXIT");
                    Console.WriteLine("\n----------------------------------------------------------");

                    Console.WriteLine("\nPLEASE ENTER THE OPTION : ");
                    int n = 0;
                    while (!int.TryParse(Console.ReadLine(), out n) || (n <= 0 || n > 8))
                    {
                        Console.WriteLine("\nPLEASE ENTER A VALID OPTION");
                    }

                    if (n == 1) //condition for currently borrowed books
                    {
                        var result = report.GetCurrentlyBorrowedBooks();

                        if (result.Count == 0)
                        {
                            Console.WriteLine("\nNO BOOKS CURRENTLY BORROWED");
                        }
                        else
                        {
                            Console.WriteLine("\nBELOW ARE THE CURRENTLY BORROWED BOOKS");

                            foreach (var r in result)
                            {
                                Console.WriteLine($"\n{r}");
                            }
                        }
                    }
                    else if (n == 2) //condition for overdue books
                    {
                        var result = report.GetOverdueBooks();

                        if (result.Count == 0)
                        {
                            Console.WriteLine("\nNO OVERDUE BOOKS FOUND");
                        }
                        else
                        {
                            Console.WriteLine("\nBELOW ARE THE OVERDUE BOOKS");

                            foreach (var r in result)
                            {
                                Console.WriteLine($"\n{r}");
                            }
                        }
                    }
                    else if (n == 3) //condition for members with pending fines
                    {
                        var result = report.GetMembersWithPendingFines();

                        if (result.Count == 0)
                        {
                            Console.WriteLine("\nNO MEMBERS WITH PENDING FINES");
                        }
                        else
                        {
                            Console.WriteLine("\nBELOW ARE THE MEMBERS WITH PENDING FINES");

                            foreach (var r in result)
                            {
                                Console.WriteLine($"\n{r}");
                            }
                        }
                    }
                    else if (n == 4) //condition for most borrowed books
                    {
                        var result = report.GetMostBorrowedBooks();

                        if (result.Count == 0)
                        {
                            Console.WriteLine("\nNO BORROWING DATA FOUND");
                        }
                        else
                        {
                            Console.WriteLine("\nBELOW ARE THE MOST BORROWED BOOKS");

                            foreach (var r in result)
                            {
                                Console.WriteLine($"\n{r}");
                            }
                        }
                    }
                    else if (n == 5) //condition for available books by category
                    {
                        Console.WriteLine("\nBELOW ARE THE CATEGORY TYPE ");
                        var memresult = other.GetAllCategory();

                        foreach (var r in memresult)
                        {
                            Console.WriteLine($"{r.Id} - {r.Categoryname}");
                        }

                        Console.WriteLine("\nPLEASE ENTER CATEGORY ID : ");
                        int categoryid = 0;
                        while (!int.TryParse(Console.ReadLine(), out categoryid) || categoryid <= 0)
                        {
                            Console.WriteLine("\nPLEASE ENTER A VALID CATEGORY ID");
                        }

                        var result = report.GetAvailableBooksByCategory(categoryid);

                        if (result.Count == 0)
                        {
                            Console.WriteLine("\nNO AVAILABLE BOOKS FOUND FOR THIS CATEGORY");
                        }
                        else
                        {
                            Console.WriteLine("\nBELOW ARE THE AVAILABLE BOOKS");

                            foreach (var r in result)
                            {
                                Console.WriteLine($"\n{r}");
                            }
                        }
                    }
                    else if (n == 6) //condition for member borrowing history
                    {
                        Console.WriteLine("\nPLEASE ENTER MEMBER ID : ");
                        int memberid = 0;
                        while (!int.TryParse(Console.ReadLine(), out memberid) || memberid <= 0)
                        {
                            Console.WriteLine("\nPLEASE ENTER A VALID MEMBER ID");
                        }

                        var result = report.GetMemberBorrowingHistory(memberid);

                        if (result.Count == 0)
                        {
                            Console.WriteLine("\nNO BORROWING HISTORY FOUND FOR THIS MEMBER");
                        }
                        else
                        {
                            Console.WriteLine("\nBELOW IS THE MEMBER BORROWING HISTORY");

                            foreach (var r in result)
                            {
                                Console.WriteLine($"\n{r}");
                            }
                        }
                    }
                    else if (n == 7) //condition for member damage report
                    {
                        var result = report.GetDamageReport();

                        if (result.Count == 0)
                        {
                            Console.WriteLine("\nNO DAMAGE REPORT EXISTS STILL NOW!");
                        }
                        else
                        {
                            Console.WriteLine("\nBELOW ARE THE MEMBER DAMAGE REPORTS");

                            foreach (var r in result)
                            {
                                Console.WriteLine($"\n{r}");
                            }
                        }
                    }
                    else if (n == 8)
                    {
                        break;
                    }
                }
                catch (InvalidInputException e)
                {
                    Console.WriteLine($"\n{e.Message}");
                }
            }
        }

        //method for creating reminder management
        public void ReminderManagement()
        {
            while (true)
            {
                try
                {
                    Console.WriteLine("\n----------------------------------------------------------");
                    Console.WriteLine("\nBELOW ARE THE CONSOLE MENUS");
                    Console.WriteLine("ENTER 1 FOR OVERDUE REMINDER");
                    Console.WriteLine("ENTER 2 FOR FINE PAYMENT REMINDER");
                    Console.WriteLine("ENTER 3 FOR EXIT");
                    Console.WriteLine("\n----------------------------------------------------------");

                    Console.WriteLine("\nPLEASE ENTER THE OPTION : ");
                    int n = 0;
                    while (!int.TryParse(Console.ReadLine(), out n) || (n <= 0 || n > 8))
                    {
                        Console.WriteLine("\nPLEASE ENTER A VALID OPTION");
                    }

                    if (n == 1) //condition for overdue books 
                    {
                        var result = report.GetOverdueBooks();

                        if (result.Count == 0)
                        {
                            Console.WriteLine("\nNO OVERDUE BOOKS FOUND");
                        }
                        else
                        {
                            Console.WriteLine("\nBELOW ARE THE OVERDUE BOOKS");

                            foreach (var r in result)
                            {
                                Console.WriteLine($"\n{r}");
                            }

                            Console.WriteLine("\nPLEASE ENTER Y TO SEND NOTIFICATION : ");
                            string value = Console.ReadLine() ?? "";

                            if (value.ToLower() == "y")
                            {
                                notification.OverDueNotification();
                            }
                            else
                            {
                                Console.WriteLine("OOPS, NO NOTIFICATION SENDED!");
                            }
                        }
                    }
                    else if (n == 2) //condition for pending fine notification
                    {
                        var result = report.GetMembersWithPendingFines();

                        if (result.Count == 0)
                        {
                            Console.WriteLine("\nNO MEMBERS WITH PENDING FINES");
                        }
                        else
                        {
                            Console.WriteLine("\nBELOW ARE THE MEMBERS WITH PENDING FINES");

                            foreach (var r in result)
                            {
                                Console.WriteLine($"\n{r}");
                            }

                            Console.WriteLine("\nPLEASE ENTER Y TO SEND NOTIFICATION : ");
                            string value = Console.ReadLine() ?? "";

                            if (value.ToLower() == "y")
                            {
                                notification.FineNotification();
                            }
                            else
                            {
                                Console.WriteLine("OOPS, NO NOTIFICATION SENDED!");
                            }
                        }
                    }
                    else if (n == 3)
                    {
                        break;
                    }
                }
                catch (InvalidInputException e)
                {
                    Console.WriteLine($"\n{e.Message}");
                }
            }
        }


        //main method
        public static void Main(String[] args)
        {
            Program program = new Program();

            while (true)
            {
                try
                {
                    Console.WriteLine("\n==========================================================");
                    Console.WriteLine("\n               LIBRARY MANAGEMENT SYSTEM                  ");
                    Console.WriteLine("\n==========================================================");
                    Console.WriteLine("\nBELOW ARE THE CONSOLE MENUS");
                    Console.WriteLine("ENTER 1 FOR MEMBER MANAGEMENT");
                    Console.WriteLine("ENTER 2 FOR BOOK MANAGEMENT");
                    Console.WriteLine("ENTER 3 FOR BORROW BOOK");
                    Console.WriteLine("ENTER 4 FOR RETURN BOOK");
                    Console.WriteLine("ENTER 5 FOR FINE MANAGEMENT");
                    Console.WriteLine("ENTER 6 FOR REPORTS");
                    Console.WriteLine("ENTER 7 FOR REMINDER MANAGEMENT");
                    Console.WriteLine("ENTER 8 FOR EXIT");
                    Console.WriteLine("\n===========================================================");

                    Console.WriteLine("\nPLEASE ENTER THE OPTION : ");
                    int n = 0;
                    while (!int.TryParse(Console.ReadLine(), out n) || (n <= 0 || n > 8))
                    {
                        Console.WriteLine("\nPLEASE ENTER A VALID OPTION");
                    }

                    if (n == 1)
                    {
                        program.MemberManagement();
                    }
                    else if (n == 2)
                    {
                        program.BookManagement();
                    }
                    else if (n == 3)
                    {
                        program.BorrowManagement();
                    }
                    else if (n == 4)
                    {
                        program.ReturnManagement();
                    }
                    else if (n == 5)
                    {
                        program.FineManagement();
                    }
                    else if (n == 6)
                    {
                        program.ReportManagement();
                    }
                    else if (n == 7)
                    {
                        program.ReminderManagement();
                    }
                    else if (n == 8)
                    {
                        break;
                    }
                }
                catch (InvalidInputException e)
                {
                    Console.WriteLine($"\n{e.Message}");
                }
            }
        }
    }
}