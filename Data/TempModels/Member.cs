using System;
using System.Collections.Generic;

namespace Data.TempModels;

public partial class Member
{
    public int Id { get; set; }

    public string? ImageName { get; set; }

    public string FirstName { get; set; } = null!;

    public string LastName { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string? Phone { get; set; }

    public DateTime? DateOfBirth { get; set; }

    public int? RoleId { get; set; }

    public int? AddressId { get; set; }

    public string? ProjectId { get; set; }

    public string? UserId { get; set; }

    public virtual Address? Address { get; set; }

    public virtual Project? Project { get; set; }

    public virtual MemberRole? Role { get; set; }

    public virtual AspNetUser? User { get; set; }
}
