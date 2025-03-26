using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Data.Entities;

public class RoleEntity
{
    public int Id { get; set; }

    [Required]
    [Column(TypeName = "nvarchar(30)")]
    public string Name { get; set; } = null!;


    public ICollection<MemberEntity> Members { get; set; } = new List<MemberEntity>();
}