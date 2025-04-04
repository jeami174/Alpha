using System.ComponentModel.DataAnnotations;

namespace Business.Models;

public class StatusFormModel
{
    [Display(Name = "Status name", Prompt = "Enter status name")]
    [Required(ErrorMessage = "Status name is required.")]
    [StringLength(50, ErrorMessage = "Status name can't be longer than 50 characters.")]
    public string StatusName { get; set; } = null!;
}
