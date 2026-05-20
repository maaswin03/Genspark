using LibraryApi.Interfaces;
using LibraryApi.Misc;
using LibraryApi.Models;

namespace LibraryApi.Services
{
    public class MemberService : IMemberService
    {
        IRepository<int, Member> _memberrepo;

        public MemberService(IRepository<int, Member> repository)
        {
            _memberrepo = repository;
        }

        //method for creating a new member
        public Member? CreateMember(Member member)
        {
            if (!ValidateMember(member))
            {
                return null;
            }
            member.FullName = member.FullName.ToUpper();
            member.Email = member.Email.ToLower();
            member.MembershipDate = DateTime.UtcNow;
            var result = _memberrepo.Create(member);
            return result;
        }

        //method for returning all the member details
        public List<Member> GetAllMembers()
        {
            return _memberrepo.GetAll();
        }

        //method for returning member by id
        public Member? GetMemberById(int id)
        {
            return _memberrepo.GetById(id);
        }

        //method for validating member details
        public bool ValidateMember(Member member)
        {
            var result = _memberrepo.GetAll();
            bool EmailExists = result.Any(m => m.Email == member.Email);
            bool PhoneNumberExists = result.Any(m => m.PhoneNumber == member.PhoneNumber);
            bool UseridExists = result.Any(m => m.MemberId == member.MemberId);

            if (string.IsNullOrWhiteSpace(member.FullName))
            {
                throw new InvalidInputException("PLEASE ENTER A VALID FULL NAME");
            }
            else if (EmailExists)
            {
                throw new InvalidInputException("OOPS, THE EMAIL ALREADY EXISTS WITH US");
            }
            else if (!member.Email.Contains("@") || !member.Email.Contains("."))
            {
                throw new InvalidInputException("PLEASE ENTER A VALID EMAIL ADDRESS");
            }
            else if (PhoneNumberExists)
            {
                throw new InvalidInputException("OOPS, THE PHONE NUMBER ALREADY EXISTS WITH US");
            }
            else if (member.PhoneNumber.Length != 10)
            {
                throw new InvalidInputException("PLEASE ENTER A VALID PHONE NUMBER");
            }
            return true;
        }
    }
}