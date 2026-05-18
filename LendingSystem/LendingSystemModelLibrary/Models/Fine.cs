using System;
using System.Collections.Generic;

namespace LendingSystemModelLibrary.Models;

public partial class Fine
{
    public int Id { get; set; }

    public int Borrowingid { get; set; }

    public int Memberid { get; set; }

    public decimal Amount { get; set; }

    public string Finereason { get; set; } = null!;

    public bool Ispaid { get; set; }

    public DateTime Createdat { get; set; }

    public DateTime? Paidat { get; set; }

    public virtual ICollection<Bookdamagereport> Bookdamagereports { get; set; } = new List<Bookdamagereport>();

    public virtual Borrowing Borrowing { get; set; } = null!;

    public virtual ICollection<Finepayment> Finepayments { get; set; } = new List<Finepayment>();

    public virtual Member Member { get; set; } = null!;
}
