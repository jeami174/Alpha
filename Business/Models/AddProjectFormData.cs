using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.RegularExpressions;
using Business.Models;

public class AddProjectFormData
{
    public AddProjectForm Form { get; set; } = new();

    public int SelectedClientId { get; set; }

    public int SelectedStatusId { get; set; }

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