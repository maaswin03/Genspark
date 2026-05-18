using System;
using System.Collections.Generic;

namespace LendingSystemModelLibrary.Models;

public partial class Finepayment
{
    public int Id { get; set; }

    public int Fineid { get; set; }

    public decimal Amountpaid { get; set; }

    public string Paymentmethod { get; set; } = null!;

    public DateTime Createdat { get; set; }

    public string Paymentstatus { get; set; } = null!;

    public virtual Fine Fine { get; set; } = null!;
}
