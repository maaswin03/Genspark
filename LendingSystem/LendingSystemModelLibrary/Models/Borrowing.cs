using System;
using System.Collections.Generic;

namespace LendingSystemModelLibrary.Models;

public partial class Borrowing
{
    public int Id { get; set; }

    public int Memberid { get; set; }

    public int Bookcopyid { get; set; }

    public DateTime Borrowdate { get; set; }

    public DateTime Duedate { get; set; }

    public DateTime? Returndate { get; set; }

    public string Borrowstatus { get; set; } = null!;

    public virtual Bookcopy Bookcopy { get; set; } = null!;

    public virtual ICollection<Bookdamagereport> Bookdamagereports { get; set; } = new List<Bookdamagereport>();

    public virtual ICollection<Fine> Fines { get; set; } = new List<Fine>();

    public virtual Member Member { get; set; } = null!;
}
