using LendingSystemDALLibrary.Contexts;
using LendingSystemDALLibrary.Interfaces;
using LendingSystemModelLibrary.Models;

namespace LendingSystemDALLibrary.Repository
{
    public class MemberRepository : IMemberRepository
    {

        BooklendingsystemContext _context;

        //initializing context with constructor
        public MemberRepository()
        {
            _context = new BooklendingsystemContext();
        }

        //method for creating new member
        public Member Create(Member member)
        {
            _context.Members.Add(member);
            _context.SaveChanges();
            return member;
        }

        //method for deactivating member
        public Member? Delete(int key)
        {
            var member = _context.Members.FirstOrDefault(m => m.Id == key);

            if (member == null) //checking member is present
            {
                return null;
            }

            member.Isactive = false; // making user in active
            _context.SaveChanges();
            return member;
        }

        //method for updating member details
        public Member? Update(Member member)
        {
            var result = _context.Members.FirstOrDefault(m => m.Id == member.Id);

            if (result == null) //checking user is present
            {
                return null;
            }

            _context.Members.Update(member); //updating membership status
            _context.SaveChanges();
            return result;
        }

        //method for getting member by Id
        public Member? GetById(int key)
        {
            return _context.Members.FirstOrDefault(m => m.Id == key); //getting member details by id
        }

        //method for getting all user details
        public List<Member> GetAll()
        {
            return _context.Members.ToList(); //get all members
        }

        //method for getting member by email
        public Member? GetByEmail(string email)
        {
            return _context.Members.FirstOrDefault(m => m.Email == email); //getting member by email
        }

        //method for getting member by phone
        public Member? GetByPhone(string phone)
        {
            return _context.Members.FirstOrDefault(m => m.Phone == phone); //getting member by phone
        }
    }
}