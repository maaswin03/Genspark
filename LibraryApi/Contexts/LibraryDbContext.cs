using LibraryApi.Models;
using Microsoft.EntityFrameworkCore;

namespace LibraryApi.Contexts
{
    public class LibraryDbContext : DbContext
    {

        public LibraryDbContext(DbContextOptions dbContextOptions) : base(dbContextOptions)
        {

        }

        public DbSet<Book> books { get; set; }
        public DbSet<Member> members { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Book>(b =>
            {
                b.HasKey(b => b.BookId); //setting primary key

            });

            modelBuilder.Entity<Member>(m =>
            {
                m.HasKey(m => m.MemberId); //setting primary key

                m.Property(m => m.MembershipDate).HasColumnType("timestamp with time zone"); //fixing time zone issues
            });
        }
    }
}