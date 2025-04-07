using System;
using System.Collections.Generic;

namespace Data.TempModels;

public partial class Address
{
    public int Id { get; set; }

    public string Street { get; set; } = null!;

    public string PostalCode { get; set; } = null!;

    public string City { get; set; } = null!;

    public virtual ICollection<Member> Members { get; set; } = new List<Member>();
}
