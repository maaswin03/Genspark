using LibraryApi.Models;

namespace LibraryApi.Interfaces
{
    public interface IMemberService
    {
        Member CreateMember(Member member); //method for creating new member

        List<Member> GetAllMembers(); //method for getting all the members

        Member? GetMemberById(int id); //method for getting member by id
    }
}