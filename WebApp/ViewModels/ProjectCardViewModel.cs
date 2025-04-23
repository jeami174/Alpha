namespace WebApp.ViewModels;

public class ProjectCardViewModel
{
    public string Id { get; set; } = null!;
    public string ProjectName { get; set; } = null!;
    public string? Description { get; set; }
    public string? ClientName { get; set; }
    public string? ImageName { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public List<string> MemberImageNames { get; set; } = [];
}