using System;
using System.Collections.Generic;
using LendingSystemModelLibrary.Models;
using Microsoft.EntityFrameworkCore;

namespace LendingSystemDALLibrary.Contexts;

public partial class BooklendingsystemContext : DbContext
{
    public BooklendingsystemContext()
    {
    }

    public BooklendingsystemContext(DbContextOptions<BooklendingsystemContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Book> Books { get; set; }

    public virtual DbSet<Bookcategory> Bookcategories { get; set; }

    public virtual DbSet<Bookcopy> Bookcopies { get; set; }

    public virtual DbSet<Bookdamagereport> Bookdamagereports { get; set; }

    public virtual DbSet<Borrowing> Borrowings { get; set; }

    public virtual DbSet<Fine> Fines { get; set; }

    public virtual DbSet<Finepayment> Finepayments { get; set; }

    public virtual DbSet<Member> Members { get; set; }

    public virtual DbSet<Membershiptype> Membershiptypes { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseNpgsql("Host=localhost;Port=5432;Database=booklendingsystem;Username=aswin;Password=Nopassword123");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Book>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("pk_book_id");

            entity.ToTable("book");

            entity.HasIndex(e => e.Isbn, "book_isbn_key").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Author)
                .HasMaxLength(50)
                .HasColumnName("author");
            entity.Property(e => e.Categoryid).HasColumnName("categoryid");
            entity.Property(e => e.Isbn)
                .HasMaxLength(20)
                .HasColumnName("isbn");
            entity.Property(e => e.Publishedyear).HasColumnName("publishedyear");
            entity.Property(e => e.Title)
                .HasMaxLength(100)
                .HasColumnName("title");

            entity.HasOne(d => d.Category).WithMany(p => p.Books)
                .HasForeignKey(d => d.Categoryid)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("fk_book_category_id");
        });

        modelBuilder.Entity<Bookcategory>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("pk_bookcategory_id");

            entity.ToTable("bookcategory");

            entity.HasIndex(e => e.Categoryname, "bookcategory_categoryname_key").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Categoryname)
                .HasMaxLength(50)
                .HasColumnName("categoryname");
        });

        modelBuilder.Entity<Bookcopy>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("pk_bookcopies_id");

            entity.ToTable("bookcopies");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Bookid).HasColumnName("bookid");
            entity.Property(e => e.Bookstatus)
                .HasMaxLength(15)
                .HasDefaultValueSql("'AVAILABLE'::character varying")
                .HasColumnName("bookstatus");
            entity.Property(e => e.Createdat)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("createdat");

            entity.HasOne(d => d.Book).WithMany(p => p.Bookcopies)
                .HasForeignKey(d => d.Bookid)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("fk_bookcopies_book_id");
        });

        modelBuilder.Entity<Bookdamagereport>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("pk_bookdamagereport_id");

            entity.ToTable("bookdamagereport");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Borrowingid).HasColumnName("borrowingid");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.Fineid).HasColumnName("fineid");
            entity.Property(e => e.Isresolved).HasColumnName("isresolved");
            entity.Property(e => e.Reportedat)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("reportedat");

            entity.HasOne(d => d.Borrowing).WithMany(p => p.Bookdamagereports)
                .HasForeignKey(d => d.Borrowingid)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("fk_bookdamagereport_borrowing_id");

            entity.HasOne(d => d.Fine).WithMany(p => p.Bookdamagereports)
                .HasForeignKey(d => d.Fineid)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("fk_bookdamagereport_fine_id");
        });

        modelBuilder.Entity<Borrowing>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("pk_borrowing_id");

            entity.ToTable("borrowing");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Bookcopyid).HasColumnName("bookcopyid");
            entity.Property(e => e.Borrowdate)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("borrowdate");
            entity.Property(e => e.Borrowstatus)
                .HasMaxLength(10)
                .HasDefaultValueSql("'BORROWED'::character varying")
                .HasColumnName("borrowstatus");
            entity.Property(e => e.Duedate)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("duedate");
            entity.Property(e => e.Memberid).HasColumnName("memberid");
            entity.Property(e => e.Returndate)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("returndate");

            entity.HasOne(d => d.Bookcopy).WithMany(p => p.Borrowings)
                .HasForeignKey(d => d.Bookcopyid)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("fk_borrowing_bookcopy_id");

            entity.HasOne(d => d.Member).WithMany(p => p.Borrowings)
                .HasForeignKey(d => d.Memberid)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("fk_borrowing_member_id");
        });

        modelBuilder.Entity<Fine>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("pk_fine_id");

            entity.ToTable("fine");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Amount)
                .HasPrecision(6, 2)
                .HasColumnName("amount");
            entity.Property(e => e.Borrowingid).HasColumnName("borrowingid");
            entity.Property(e => e.Createdat)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("createdat");
            entity.Property(e => e.Finereason)
                .HasMaxLength(20)
                .HasColumnName("finereason");
            entity.Property(e => e.Ispaid).HasColumnName("ispaid");
            entity.Property(e => e.Memberid).HasColumnName("memberid");
            entity.Property(e => e.Paidat)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("paidat");

            entity.HasOne(d => d.Borrowing).WithMany(p => p.Fines)
                .HasForeignKey(d => d.Borrowingid)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("fk_fine_borrowing_id");

            entity.HasOne(d => d.Member).WithMany(p => p.Fines)
                .HasForeignKey(d => d.Memberid)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("fk_fine_member_id");
        });

        modelBuilder.Entity<Finepayment>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("pk_finepayment_id");

            entity.ToTable("finepayment");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Amountpaid)
                .HasPrecision(6, 2)
                .HasColumnName("amountpaid");
            entity.Property(e => e.Createdat)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("createdat");
            entity.Property(e => e.Fineid).HasColumnName("fineid");
            entity.Property(e => e.Paymentmethod)
                .HasMaxLength(10)
                .HasColumnName("paymentmethod");
            entity.Property(e => e.Paymentstatus)
                .HasMaxLength(50)
                .HasDefaultValueSql("'SUCCESS'::character varying")
                .HasColumnName("paymentstatus");

            entity.HasOne(d => d.Fine).WithMany(p => p.Finepayments)
                .HasForeignKey(d => d.Fineid)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("fk_finepayment_fine_id");
        });

        modelBuilder.Entity<Member>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("pk_member_id");

            entity.ToTable("member");

            entity.HasIndex(e => e.Email, "member_email_key").IsUnique();

            entity.HasIndex(e => e.Phone, "member_phone_key").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Createdat)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("createdat");
            entity.Property(e => e.Email)
                .HasMaxLength(50)
                .HasColumnName("email");
            entity.Property(e => e.Isactive)
                .HasDefaultValue(true)
                .HasColumnName("isactive");
            entity.Property(e => e.Membershiptypeid).HasColumnName("membershiptypeid");
            entity.Property(e => e.Name)
                .HasMaxLength(50)
                .HasColumnName("name");
            entity.Property(e => e.Password)
                .HasMaxLength(50)
                .HasColumnName("password");
            entity.Property(e => e.PasswordSet).HasColumnName("password_set");
            entity.Property(e => e.Phone)
                .HasMaxLength(15)
                .HasColumnName("phone");

            entity.HasOne(d => d.Membershiptype).WithMany(p => p.Members)
                .HasForeignKey(d => d.Membershiptypeid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_member_membershiptype_id");
        });

        modelBuilder.Entity<Membershiptype>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("pk_membershiptype_id");

            entity.ToTable("membershiptype");

            entity.HasIndex(e => e.Typename, "membershiptype_typename_key").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Maxborrowdays).HasColumnName("maxborrowdays");
            entity.Property(e => e.Maxborrowlimit).HasColumnName("maxborrowlimit");
            entity.Property(e => e.Typename)
                .HasMaxLength(20)
                .HasColumnName("typename");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
