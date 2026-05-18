using LendingSystemBLLibrary.Interface;
using LendingSystemDALLibrary.Interfaces;
using LendingSystemDALLibrary.Repository;
using LendingSystemModelLibrary.Exceptions;
using LendingSystemModelLibrary.Models;

namespace LendingSystemBLLibrary.Service
{
    public class OtherService : IOtherService
    {
        IRepository<int, Bookcategory> _categoryrepo;

        IRepository<int, Membershiptype> _membershiprepo;

        public OtherService()
        {
            _categoryrepo = new BookCategoryRepository();
            _membershiprepo = new MembershipTypeRepository();
        }

        //method for creating a new category
        public Bookcategory CreateCategory(string name)
        {
            var result = _categoryrepo.GetAll().Any(u => u.Categoryname == name);

            if (result || name.Length == 0) //check category already exists or not 
            {
                throw new InvalidInputException("OOPS, CATEGORY NAME IS NOT VALID!");
            }

            Bookcategory category = new Bookcategory();
            category.Categoryname = name;
            return _categoryrepo.Create(category);
        }

        //method for getting all the category
        public List<Bookcategory> GetAllCategory()
        {
            var result = _categoryrepo.GetAll();

            if (result.Count == 0)
            {
                throw new InvalidInputException("OOPS, NO CATEGORY CURRENTLY PRESENT!");
            }

            return result;
        }

        //method for getting all the membership types
        public List<Membershiptype> GetAllMembership()
        {
            var result = _membershiprepo.GetAll();

            if (result.Count == 0)
            {
                throw new InvalidInputException("OOPS, NO MEMBERSHIP PRESENT CURRENTLY");
            }

            return result;
        }
    }
}