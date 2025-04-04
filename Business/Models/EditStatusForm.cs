using System.ComponentModel.DataAnnotations;

namespace Business.Models;

public class EditStatusForm
{
    public int Id { get; set; }

    [Display(Name = "Status name", Prompt = "Edit status name")]
    [Required(ErrorMessage = "Status name is required.")]
    [StringLength(50, ErrorMessage = "Status name can't be longer than 50 characters.")]
    public string StatusName { get; set; } = null!;
}
