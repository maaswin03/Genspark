using LendingSystemBLLibrary.Interface;
using LendingSystemDALLibrary.Contexts;
using LendingSystemDALLibrary.Interfaces;
using LendingSystemDALLibrary.Repository;
using LendingSystemModelLibrary.Exceptions;
using LendingSystemModelLibrary.Models;
using Microsoft.EntityFrameworkCore;

namespace LendingSystemBLLibrary.Service
{
    public class FineService : IFineService
    {

        IRepository<int, Fine> _finerepo;
        IRepository<int, Finepayment> _finepayment;
        BooklendingsystemContext _context;


        public FineService()
        {
            _finerepo = new FineRepository();
            _finepayment = new FinePaymentRepository();
            _context = new BooklendingsystemContext();
        }

        //method for fetching all the pending fines
        public List<Fine> GetAllFineById(int id)
        {
            var result = _finerepo.GetAll().Where(f => f.Memberid == id && f.Ispaid == false).ToList();

            if (result.Count == 0)
            {
                throw new InvalidInputException("NO PENDING FINE PRESENT FOR THE MEMBER!");
            }

            return result;
        }

        //method for getting member pending fines
        public decimal GetFineByMemberId(int memberid)
        {
            var result = _context.Database.ExecuteSqlInterpolated($"SELECT calculate_member_fine({memberid})");
            return result;
        }

        //method for fetching all the user fine
        public List<Fine> GetAllFine(int id)
        {
            var result = _finerepo.GetAll().Where(f => f.Memberid == id).ToList();

            if (result.Count == 0)
            {
                throw new InvalidInputException("NO FINE HISTORY PRESENT FOR THE MEMBER!");
            }

            return result;
        }

        //method for paying fine
        public Finepayment? PayFine(int id, string method)
        {
            var result = _finerepo.Delete(id);

            if (result != null)
            {
                Finepayment finepayment = new Finepayment();
                finepayment.Fineid = id;
                finepayment.Amountpaid = result.Amount;
                finepayment.Paymentmethod = method;

                return _finepayment.Create(finepayment);
            }

            return null;
        }
    }
}