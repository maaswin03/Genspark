using LendingSystemDALLibrary.Contexts;
using LendingSystemDALLibrary.Interfaces;
using LendingSystemModelLibrary.Models;

namespace LendingSystemDALLibrary.Repository
{
    public class BookDamageRepository : IRepository<int, Bookdamagereport>
    {
        BooklendingsystemContext _context;

        public BookDamageRepository()
        {
            _context = new BooklendingsystemContext();
        }

        //method for creating a new damage report
        public Bookdamagereport Create(Bookdamagereport report)
        {
            _context.Bookdamagereports.Add(report);
            _context.SaveChanges();
            return report;
        }

        //method for deleting damage report
        public Bookdamagereport? Delete(int key)
        {
            var result = _context.Bookdamagereports.FirstOrDefault(r => r.Id == key);

            if (result == null)
            {
                return null;
            }

            result.Isresolved = true;
            _context.SaveChanges();
            return result;
        }

        //method for getting report by id
        public Bookdamagereport? GetById(int key)
        {
            return _context.Bookdamagereports.FirstOrDefault(r => r.Id == key);
        }

        //method for getting all report details
        public List<Bookdamagereport> GetAll()
        {
            return _context.Bookdamagereports.ToList();
        }

        //method for updating report details
        public Bookdamagereport? Update(Bookdamagereport report)
        {
            var result = _context.Bookdamagereports.FirstOrDefault(r => r.Id == report.Id);

            if (result == null)
            {
                return null;
            }

            _context.Bookdamagereports.Update(report);
            _context.SaveChanges();
            return report;
        }
    }
}