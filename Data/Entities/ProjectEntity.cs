using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Data.Entities;
public class ProjectEntity
{
    [Key]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    public string? ImageName { get; set; }

    [Column(TypeName = "nvarchar(100)")]
    public string ProjectName { get; set; } = null!;

    [Column(TypeName = "nvarchar(300)")]
    public string? Description { get; set; }

    [Column(TypeName = "date")]
    public DateTime StartDate { get; set; }

    [Column(TypeName = "date")]
    public DateTime? EndDate { get; set; }

    public DateTime Created {  get; set; } = DateTime.Now;

    public decimal? Budget { get; set; }

    public int StatusId { get; set; }

    public int ClientId { get; set; }

    [ForeignKey(nameof(ClientId))]
    public ClientEntity Client { get; set; } = null!;

    public ICollection<MemberEntity> Members { get; set; } = [];
}
