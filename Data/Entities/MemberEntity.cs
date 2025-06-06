﻿
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Data.Entities;
public class MemberEntity
{
    [Key]
    public int Id { get; set; }

    public string? ImageName { get; set; }

    [Column(TypeName = "nvarchar(100)")]
    public string FirstName { get; set; } = null!;

    [Column(TypeName = "nvarchar(100)")]
    public string LastName { get; set; } = null!;

    [Column(TypeName = "varchar(256)")]
    public string Email { get; set; } = null!;

    [Column(TypeName = "varchar(20)")]
    public string? Phone { get; set; }

    public DateTime? DateOfBirth { get; set; }

    public int? RoleId { get; set; }

    public int? AddressId { get; set; }

    public string? UserId { get; set; }

    [ForeignKey(nameof(RoleId))]
    public RoleEntity? Role { get; set; }

    [ForeignKey(nameof(AddressId))]
    public AddressEntity? Address { get; set; }

    [ForeignKey(nameof(UserId))]
    public virtual ApplicationUser? User { get; set; } //Ok med virtual? 

    public ICollection<ProjectEntity> Projects { get; set; } = new List<ProjectEntity>();

    [NotMapped]
    public string FullName => $"{FirstName} {LastName}";

    [Column(TypeName = "datetime2")]
    public DateTime Created { get; set; } = DateTime.Now;

}
