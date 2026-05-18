using LendingSystemDALLibrary.Contexts;
using LendingSystemDALLibrary.Interfaces;
using LendingSystemModelLibrary.Models;

namespace LendingSystemDALLibrary.Repository
{
    public class FinePaymentRepository : IRepository<int, Finepayment>
    {
        BooklendingsystemContext _context;

        //initializing Context
        public FinePaymentRepository()
        {
            _context = new BooklendingsystemContext();
        }

        //method for creating a new payment
        public Finepayment Create(Finepayment fine)
        {
            _context.Finepayments.Add(fine);
            _context.SaveChanges();
            return fine;
        }

        //method for updating fine payment into cancelled
        public Finepayment? Delete(int key)
        {
            var result = _context.Finepayments.FirstOrDefault(f => f.Id == key);

            if(result == null)
            {
                return null;
            }

            result.Paymentstatus = "CANCELLED";
            _context.SaveChanges();
            return result;
        }

        //method for updating payment
        public Finepayment? Update(Finepayment fine)
        {
            var result = _context.Finepayments.FirstOrDefault(f => f.Id == fine.Id);

            if (result == null)
            {
                return null;
            }

            _context.Finepayments.Update(fine);
            _context.SaveChanges();
            return fine;
        }

        //method for getting fine payment by Id
        public Finepayment? GetById(int key)
        {
            return _context.Finepayments.FirstOrDefault(f => f.Id == key);
        }

        //method for getting all fine payment
        public List<Finepayment> GetAll()
        {
            return _context.Finepayments.ToList();
        }
    }
}