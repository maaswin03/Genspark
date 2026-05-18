using System;
using System.Collections.Generic;

namespace LendingSystemModelLibrary.Models;

public partial class Bookcopy
{
    public int Id { get; set; }

    public int Bookid { get; set; }

    public string Bookstatus { get; set; } = null!;

    public DateTime Createdat { get; set; }

    public virtual Book Book { get; set; } = null!;

    public virtual ICollection<Borrowing> Borrowings { get; set; } = new List<Borrowing>();
}
