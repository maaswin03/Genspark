using System;
using System.Collections.Generic;

namespace LendingSystemModelLibrary.Models;

public partial class Member
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string Phone { get; set; } = null!;

    public bool Isactive { get; set; }

    public int Membershiptypeid { get; set; }

    public DateTime Createdat { get; set; }

    public string Password { get; set; } = null!;

    public bool PasswordSet { get; set; }

    public virtual ICollection<Borrowing> Borrowings { get; set; } = new List<Borrowing>();

    public virtual ICollection<Fine> Fines { get; set; } = new List<Fine>();

    public virtual Membershiptype Membershiptype { get; set; } = null!;
}
