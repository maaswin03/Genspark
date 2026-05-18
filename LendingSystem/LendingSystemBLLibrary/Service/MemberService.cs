using LendingSystemBLLibrary.Interface;
using LendingSystemDALLibrary.Interfaces;
using LendingSystemDALLibrary.Repository;
using LendingSystemModelLibrary.Exceptions;
using LendingSystemModelLibrary.Models;

namespace LendingSystemBLLibrary.Service
{
    public class MemberService : IMemberService
    {
        IMemberRepository _memberrepo;
        IRepository<int, Borrowing> _borrowrepo;
        IRepository<int, Fine> _finerepo;

        //constructor for initializing values
        public MemberService()
        {
            _memberrepo = new MemberRepository();
            _borrowrepo = new BorrowingRepository();
            _finerepo = new FineRepository();
        }

        //method for creating new members 
        public Member? CreateMember(string name, string email, string phone, int membershipid)
        {
            //creating member object with all the fields
            Member member = new Member();
            member.Name = name;
            member.Email = email;
            member.Phone = phone;
            member.Isactive = true;
            member.Membershiptypeid = membershipid;
            member.Password = "lending@123";
            member.PasswordSet = false;
            member.Createdat = DateTime.Now;

            if (!ValidateMember(member))
            {
                return null;
            }

            return _memberrepo.Create(member);
        }

        //method for viewing all the member details
        public List<Member> ViewAllMember()
        {
            var result = _memberrepo.GetAll();

            if (result.Count == 0)
            {
                throw new InvalidInputException("OOPS, NO MEMBER PRESENT CURRENTLY");
            }

            return result;
        }

        //method for fetching member by Id
        public Member ViewMemberById(int id)
        {
            var result = _memberrepo.GetById(id);

            if (result == null)
            {
                throw new InvalidInputException("OOPS, NO MEMBER EXISTS WITH THAT ID!");
            }
            return result;
        }

        //method for fetching member by email
        public Member ViewMemberByEmail(string email)
        {
            var result = _memberrepo.GetByEmail(email);

            if (result == null)
            {
                throw new InvalidInputException("OOPS, NO MEMBER EXISTS WITH THAT EMAIL ID!");
            }
            return result;
        }

        //method for fetching member by phone
        public Member ViewMemberByPhone(string phone)
        {
            var result = _memberrepo.GetByPhone(phone);

            if (result == null)
            {
                throw new InvalidInputException("OOPS, NO MEMBER EXISTS WITH THAT PHONE NUMBER!");
            }
            return result;
        }

        //method for updating membership status
        public Member? UpdateMemberShipStatus(int memberid, int membershipid)
        {
            var result = ViewMemberById(memberid);

            if (membershipid <= 0 || membershipid > 3)
            {
                throw new InvalidInputException("PLEASE ENTER A VALID MEMBERSHIP ID");
            }

            result.Membershiptypeid = membershipid;
            return _memberrepo.Update(result);
        }

        //method for deactivating account
        public Member? DeactivateMember(int id)
        {
            var result = _memberrepo.GetById(id);

            if (result == null)
            {
                throw new InvalidInputException("OOPS, NO MEMBER EXISTS WITH THAT ID!");
            }

            if (!ValidateDeactivateMember(id))
            {
                return null;
            }

            return _memberrepo.Delete(id);
        }

        //method for validating Deactivating Member
        public bool ValidateDeactivateMember(int id)
        {
            bool BorrowingExists = _borrowrepo.GetAll().Any(b => b.Memberid == id && b.Borrowstatus == "BORROWED");
            bool FineExists = _finerepo.GetAll().Any(f => f.Memberid == id && f.Ispaid == false);

            if (BorrowingExists)
            {
                throw new InvalidInputException("OOPS, THE MEMBER NOT RETURNED THE BORROWED BOOK!");
            }
            else if (FineExists)
            {
                throw new InvalidInputException("OOPS, THE MEMBER HAS SOME PENDING FINES!");
            }
            return true;
        }

        //method for validating all the member details
        public bool ValidateMember(Member member)
        {
            var allMember = _memberrepo.GetAll();
            bool MemberEmail = false;
            bool MemberPhone = false;

            //check existing details
            if (allMember.Count > 0)
            {
                //using LINQ for checking email or phone already present
                MemberEmail = allMember.Any(x => x.Email == member.Email);
                MemberPhone = allMember.Any(x => x.Phone == member.Phone);
            }

            if (MemberEmail)
            {
                throw new InvalidInputException("OOPS, EMAIL ID ALREADY EXISTS WITH US!");
            }
            else if (MemberPhone)
            {
                throw new InvalidInputException("OOPS, PHONE NUMBER ALREADY EXISTS WITH US!");
            }
            else if (string.IsNullOrWhiteSpace(member.Name))
            {
                throw new InvalidInputException("PLEASE ENTER A VALID NAME!");
            }
            else if (!member.Email.Contains('@') || !member.Email.Contains('.'))
            {
                throw new InvalidInputException("PLEASE ENTER A VALID EMAIL!");
            }
            else if (member.Phone.Length != 10)
            {
                throw new InvalidInputException("PLEASE ENTER A VALID PHONE NUMBER!");
            }
            else if (member.Membershiptypeid > 3 || member.Membershiptypeid <= 0)
            {
                throw new InvalidInputException("PLEASE ENTER A VALID MEMBERSHIP ID");
            }
            return true;
        }
    }
}