
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Business.Models;

public class EditProjectFormData
{
    public EditProjectForm Form { get; set; } = new();

    public int SelectedClientId { get; set; }

    [Required(ErrorMessage = "You must select at least one member.")]
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