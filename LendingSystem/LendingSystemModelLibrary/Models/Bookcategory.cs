using System;
using System.Collections.Generic;

namespace LendingSystemModelLibrary.Models;

public partial class Bookcategory
{
    public int Id { get; set; }

    public string Categoryname { get; set; } = null!;

    public virtual ICollection<Book> Books { get; set; } = new List<Book>();
}
