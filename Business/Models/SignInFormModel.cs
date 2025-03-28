using System.ComponentModel.DataAnnotations;

namespace Business.Models;

public class SignInFormModel
{
    [Display(Name = "Email", Prompt = "Your email address")]
    [DataType(DataType.EmailAddress)]
    [Required(ErrorMessage = "Required")]
    public string Email { get; set; } = null!;

    [Display(Name = "Password", Prompt = "Enter your password")]
    [DataType(DataType.Password)]
    [Required(ErrorMessage = "Required")]
    public string Password { get; set; } = null!;
    public bool RememberMe { get; set; }
}
