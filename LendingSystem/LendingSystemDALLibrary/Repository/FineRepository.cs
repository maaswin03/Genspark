using LendingSystemDALLibrary.Contexts;
using LendingSystemDALLibrary.Interfaces;
using LendingSystemModelLibrary.Models;

namespace LendingSystemDALLibrary.Repository
{
    public class FineRepository : IRepository<int , Fine>
    {
        BooklendingsystemContext _context;

        public FineRepository()
        {
            _context = new BooklendingsystemContext();
        }

        //method for creating a new fine
        public Fine Create(Fine fine)
        {
            _context.Fines.Add(fine);
            _context.SaveChanges();
            return fine;
        }

        //method for updating payment in fine
        public Fine? Delete(int key)
        {
            var result = _context.Fines.FirstOrDefault(f => f.Id == key);

            if(result == null)
            {
                return null;
            }

            result.Ispaid = true;
            result.Paidat = DateTime.Now;
            _context.SaveChanges();
            return result;
        }

        //method for updating fine
        public Fine? Update(Fine fine)
        {
            var result = _context.Fines.FirstOrDefault(f => f.Id == fine.Id);

            if(result == null)
            {
                return null;
            }

            _context.Fines.Update(fine);
            _context.SaveChanges();
            return fine;
        }

        //method for getting fine by Id
        public Fine? GetById(int key)
        {
            return _context.Fines.FirstOrDefault(f => f.Id == key);
        }

        //method for getting all the Fines
        public List<Fine> GetAll()
        {
            return _context.Fines.ToList();
        }
    }
}