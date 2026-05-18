using LendingSystemModelLibrary.Models;

namespace LendingSystemBLLibrary.Interface
{
    public interface IMemberService
    {
        //method for creating new members 
        Member? CreateMember(string name, string email, string phone, int membershipid);

        //method for viewing all the member details
        List<Member> ViewAllMember();

        //method for fetching member by Id
        Member ViewMemberById(int id);

        //method for fetching member by email
        Member ViewMemberByEmail(string email);

        //method for fetching member by phone
        Member ViewMemberByPhone(string phone);

        //method for updating membership status
        Member? UpdateMemberShipStatus(int memberid, int membershipid);

        //method for deactivating account
        Member? DeactivateMember(int id);
    }
}
