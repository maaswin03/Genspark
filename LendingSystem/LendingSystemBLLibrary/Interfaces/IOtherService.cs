using LendingSystemModelLibrary.Models;

namespace LendingSystemBLLibrary.Interface
{
    public interface IOtherService
    {
        Bookcategory CreateCategory(string name); //method for creating a new category

        List<Bookcategory> GetAllCategory(); //method for getting all the category

        List<Membershiptype> GetAllMembership(); //method for getting all the membership types
    }
}