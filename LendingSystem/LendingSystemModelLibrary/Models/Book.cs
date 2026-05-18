using System;
using System.Collections.Generic;

namespace LendingSystemModelLibrary.Models;

public partial class Book
{
    public int Id { get; set; }

    public string Title { get; set; } = null!;

    public string Isbn { get; set; } = null!;

    public string Author { get; set; } = null!;

    public int Publishedyear { get; set; }

    public int Categoryid { get; set; }

    public virtual ICollection<Bookcopy> Bookcopies { get; set; } = new List<Bookcopy>();

    public virtual Bookcategory Category { get; set; } = null!;
}
