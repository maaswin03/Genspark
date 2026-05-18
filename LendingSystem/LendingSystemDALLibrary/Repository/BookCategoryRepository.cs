using LendingSystemDALLibrary.Contexts;
using LendingSystemDALLibrary.Interfaces;
using LendingSystemModelLibrary.Models;

namespace LendingSystemDALLibrary.Repository
{
    public class BookCategoryRepository : IRepository<int, Bookcategory>
    {
        BooklendingsystemContext _context;

        public BookCategoryRepository()
        {
            _context = new BooklendingsystemContext();
        }

        //method for creating a new book category
        public Bookcategory Create(Bookcategory category)
        {
            _context.Bookcategories.Add(category);
            _context.SaveChanges();
            return category;
        }

        //method for deleting a book category
        public Bookcategory? Delete(int key)
        {
            var category = _context.Bookcategories.FirstOrDefault(b => b.Id == key);

            if (category == null)
            {
                return null;
            }

            _context.Bookcategories.Remove(category);
            _context.SaveChanges();
            return category;
        }

        //method for updating Categoryname
        public Bookcategory? Update(Bookcategory category)
        {
            var result = _context.Bookcategories.FirstOrDefault(b => b.Id == category.Id);

            if (result == null)
            {
                return null;
            }

            result.Categoryname = category.Categoryname;
            _context.SaveChanges();
            return result;
        }

        //method for getting Bookcategory by name
        public Bookcategory? GetById(int key)
        {
            return _context.Bookcategories.FirstOrDefault(b => b.Id == key);
        }

        //method for getting all Bookcategory
        public List<Bookcategory> GetAll()
        {
            return _context.Bookcategories.ToList();
        }
    }
}