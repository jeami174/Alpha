
using System.ComponentModel.DataAnnotations.Schema;

namespace Business.Models;

public class EditProjectFormData
{
    public EditProjectForm Form { get; set; } = new();

    public int SelectedClientId { get; set; }

    public int SelectedStatusId { get; set; }

    public List<int> SelectedMemberIds { get; set; } = new();

    [NotMapped]
    public string SelectedMemberIdsRaw
    {
        get => string.Join(",", SelectedMemberIds);
        set => SelectedMemberIds = string.IsNullOrWhiteSpace(value)
            ? new List<int>()
            : value.Split(',').Select(int.Parse).ToList();
    }
}