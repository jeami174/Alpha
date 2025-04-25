using System.ComponentModel.DataAnnotations.Schema;
using Business.Models;

public class AddProjectFormData
{
    public AddProjectForm Form { get; set; } = new();

    public int SelectedClientId { get; set; }

    public int SelectedStatusId { get; set; }

    public List<int> SelectedMemberIds { get; set; } = new();

    [NotMapped] // if you are using EF
    public string SelectedMemberIdsRaw
    {
        get => string.Join(",", SelectedMemberIds);
        set => SelectedMemberIds = string.IsNullOrWhiteSpace(value)
            ? new List<int>()
            : value.Split(',').Select(int.Parse).ToList();
    }
}