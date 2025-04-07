using System;
using System.Collections.Generic;

namespace Data.TempModels;

public partial class Project
{
    public string Id { get; set; } = null!;

    public string? ImageName { get; set; }

    public string ProjectName { get; set; } = null!;

    public string? Description { get; set; }

    public DateOnly StartDate { get; set; }

    public DateOnly? EndDate { get; set; }

    public DateTime Created { get; set; }

    public decimal? Budget { get; set; }

    public int StatusId { get; set; }

    public int ClientId { get; set; }

    public virtual Client Client { get; set; } = null!;

    public virtual ICollection<Member> Members { get; set; } = new List<Member>();

    public virtual Status Status { get; set; } = null!;
}
