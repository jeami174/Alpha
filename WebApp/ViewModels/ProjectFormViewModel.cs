using Business.Models;
using Domain.Models;

namespace WebApp.ViewModels;

public class ProjectFormViewModel
{
    public AddProjectForm Form { get; set; } = new();

    public List<StatusModel> Statuses { get; set; } = [];
    public List<ClientModel> Clients { get; set; } = [];
    public List<MemberModel> Members { get; set; } = [];

    public int SelectedStatusId { get; set; }
    public int SelectedClientId { get; set; }
    public List<int> SelectedMemberIds { get; set; } = [];
}