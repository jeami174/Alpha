using Business.Models;

public class AddProjectFormData
{
    public AddProjectForm Form { get; set; } = new();

    public int SelectedClientId { get; set; }

    public int SelectedStatusId { get; set; }

    public List<int> SelectedMemberIds { get; set; } = new();
}