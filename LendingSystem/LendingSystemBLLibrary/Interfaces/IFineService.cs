using LendingSystemModelLibrary.Models;

namespace LendingSystemBLLibrary.Interface
{
    public interface IFineService
    {
        List<Fine> GetAllFineById(int id);//method for fetching all the pending fines

        List<Fine> GetAllFine(int id);//method for fetching all the user fine

        decimal GetFineByMemberId(int memberid); //method for getting member pending fines

        Finepayment? PayFine(int id, string method);//method for paying fine
    }
}