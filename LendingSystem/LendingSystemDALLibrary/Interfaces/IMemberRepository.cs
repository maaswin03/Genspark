using LendingSystemModelLibrary.Models;

namespace LendingSystemDALLibrary.Interfaces
{
    public interface IMemberRepository : IRepository<int, Member>
    {
        Member? GetByEmail(string email); //get member details by email

        Member? GetByPhone(string phone); //get member details by phone number
    }
}