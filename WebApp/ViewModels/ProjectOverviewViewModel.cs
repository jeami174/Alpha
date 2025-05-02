using Business.Models;
using Domain.Models;

namespace WebApp.ViewModels;

public class ProjectOverviewViewModel
{
    public List<ProjectCardViewModel> Projects { get; set; } = new();

    public AddProjectForm AddProjectForm { get; set; } = new();

    public EditProjectForm EditProjectForm { get; set; } = new();

    public string? SelectedClient { get; set; }
    public string? SortBy { get; set; }

    public IEnumerable<ClientModel> Clients { get; set; } = [];
    public IEnumerable<MemberModel> Members { get; set; } = [];

    public int TotalCount => Projects.Count;

    public int StartedCount => Projects.Count(p =>
        p.EndDate.HasValue && p.EndDate.Value.Date >= DateTime.Today);

    public int CompletedCount => Projects.Count(p =>
        p.EndDate.HasValue && p.EndDate.Value.Date < DateTime.Today);
}
