using System;
using System.Collections.Generic;

namespace Data.TempModels;

public partial class MemberRole
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<Member> Members { get; set; } = new List<Member>();
}
