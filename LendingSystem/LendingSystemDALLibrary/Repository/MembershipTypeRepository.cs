using LendingSystemDALLibrary.Contexts;
using LendingSystemDALLibrary.Interfaces;
using LendingSystemModelLibrary.Models;

namespace LendingSystemDALLibrary.Repository
{
    public class MembershipTypeRepository : IRepository<int, Membershiptype>
    {
        BooklendingsystemContext _context;

        public MembershipTypeRepository()
        {
            _context = new BooklendingsystemContext();
        }

        //method for creating new membership
        public Membershiptype Create(Membershiptype type)
        {
            _context.Membershiptypes.Add(type);
            _context.SaveChanges();
            return type;
        }

        //method for deleting membership type
        public Membershiptype? Delete(int key)
        {
            var result = _context.Membershiptypes.FirstOrDefault(t => t.Id == key);

            if (result == null)
            {
                return null;
            }

            _context.Membershiptypes.Remove(result);
            _context.SaveChanges();
            return result;
        }

        //method for getting all data
        public List<Membershiptype> GetAll()
        {
            return _context.Membershiptypes.ToList();
        }

        //method for getting data by id
        public Membershiptype? GetById(int key)
        {
            return _context.Membershiptypes.FirstOrDefault(m => m.Id == key);
        }

        //method for updating the membership type
        public Membershiptype? Update(Membershiptype type)
        {
            var result = _context.Membershiptypes.FirstOrDefault(t => t.Id == type.Id);

            if (result == null)
            {
                return null;
            }

            _context.Membershiptypes.Update(type);
            _context.SaveChanges();
            return result;
        }
    }
}