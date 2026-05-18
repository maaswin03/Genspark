using System;
using System.Collections.Generic;

namespace LendingSystemModelLibrary.Models;

public partial class Bookdamagereport
{
    public int Id { get; set; }

    public int Borrowingid { get; set; }

    public int? Fineid { get; set; }

    public string Description { get; set; } = null!;

    public DateTime Reportedat { get; set; }

    public bool Isresolved { get; set; }

    public virtual Borrowing Borrowing { get; set; } = null!;

    public virtual Fine? Fine { get; set; }
}
