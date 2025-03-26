using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Data.Entities;

public class AddressEntity
{
    public int Id { get; set; }

    [Required]
    [Column(TypeName = "nvarchar(50)")]
    public string Street { get; set; } = null!;

    [Required]
    [Column(TypeName = "nvarchar(10)")]
    public string PostalCode { get; set; } = null!;

    [Required]
    [Column(TypeName = "nvarchar(20)")]
    public string City { get; set; } = null!;


    public ICollection<MemberEntity> Members { get; set; } = new List<MemberEntity>();
}