using LendingSystemModelLibrary.Models;

namespace LendingSystemBLLibrary.Interface
{
    public interface IBookCopyService
    {
        //method for creating new books copies
        bool CreateBookCopies(int bookid, int noofcopies);

        //method for updating bookcopy by id
        Bookcopy? UpdateBookStatus(int bookcopyid, string status);

        //method for viewing all book copies
        List<Bookcopy> ViewAllBookCopies();

        //method for viewing book copy by id 
        Bookcopy? ViewBookCopyById(int id);
    }
}
