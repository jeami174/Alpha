using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Data.Entities;

[Index(nameof(ClientName), IsUnique = true)]
public class ClientEntity
{
    [Key]
    public int ClientId { get; set; }

    [Column(TypeName = "nvarchar(50)")]
    public string ClientName { get; set; } = null!;


    [Column(TypeName = "varchar(200)")]
    public string ClientEmail { get; set; } = null!;


    [Column(TypeName = "nvarchar(50)")]
    public string? Location { get; set; }


    [Column(TypeName = "varchar(20)")]
    public string? Phone { get; set; }

    public string? ImageName { get; set; }

    public virtual ICollection<ProjectEntity> Projects { get; set; } = [];
}
