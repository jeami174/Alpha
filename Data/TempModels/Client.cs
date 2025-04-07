using System;
using System.Collections.Generic;

namespace Data.TempModels;

public partial class Client
{
    public int ClientId { get; set; }

    public string ClientName { get; set; } = null!;

    public string ClientEmail { get; set; } = null!;

    public string? Location { get; set; }

    public string? Phone { get; set; }

    public virtual ICollection<Project> Projects { get; set; } = new List<Project>();
}
