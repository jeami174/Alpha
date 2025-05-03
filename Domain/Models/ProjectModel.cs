using Business.Models;

namespace Domain.Models;
public class ProjectModel
{
    public string Id { get; set; } = null!;
    public string? ImageName { get; set; }
    public string ProjectName { get; set; } = null!;
    public string? Description { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public DateTime Created { get; set; }
    public decimal? Budget { get; set; }
    public ClientModel ClientModel { get; set; } = null!;
    public StatusModel StatusModel { get; set; } = null!;
    public List<MemberModel> MemberModels { get; set; } = new();
}