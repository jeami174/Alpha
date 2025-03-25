
using System.ComponentModel.DataAnnotations.Schema;

namespace Data.Entities;

public class MemberEntity
{
    public int Id { get; set; }
    public string? ImageName { get; set; }

    [Column(TypeName = "nvarchar(100)")]
    public string FirstName { get; set; } = null!;

    [Column(TypeName = "nvarchar(100)")]
    public string LastName { get; set; } = null!;

    [Column(TypeName = "varchar(20)")]
    public string Email { get; set; } = null!;

    [Column(TypeName = "nvarchar(100)")]
    public string? Address { get; set; }

    [Column(TypeName = "varchar(20)")]
    public string? Phone { get; set; }

    [Column(TypeName = "nvarchar(50)")]
    public string? JobTitle { get; set; }

    public DateTime? DateOfBirth { get; set; }

}
