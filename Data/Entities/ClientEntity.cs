using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Data.Entities;

public class ClientEntity
{
    [Key]
    public int ClientId { get; set; }

    [Column(TypeName = "nvarchar(50)")]
    public string ClientName { get; set; } = null!;


    [Column(TypeName = "varchar(200)")]
    public string ClientEmail { get; set; } = null!;


    [Column(TypeName = "nvarchar(50)")]
    public string? Location { get; set; } = null!;


    [Column(TypeName = "varchar(20)")]
    public string? Phone { get; set; }
}
