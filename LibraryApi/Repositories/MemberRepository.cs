using LibraryApi.Contexts;
using LibraryApi.Interfaces;
using LibraryApi.Models;

namespace LibraryApi.Repositories
{
    public class MemberRepository : IRepository<int, Member>
    {
        protected readonly LibraryDbContext _context;

        public MemberRepository(LibraryDbContext context)
        {
            _context = context;
        }

        //method for creating new member
        public Member Create(Member member)
        {
            _context.members.Add(member);
            _context.SaveChanges();
            return member;
        }

        //method for getting all member details
        public List<Member> GetAll()
        {
            return _context.members.ToList();
        }

        //method for getting member by id
        public Member? GetById(int key)
        {
            return _context.members.FirstOrDefault(m => m.MemberId == key);
        }
    }
}