using System.ComponentModel.DataAnnotations;

namespace Business.Models;

public class ForgotPasswordFormModel
{
    [Required(ErrorMessage = "You must enter your email")]
    [DataType(DataType.EmailAddress)]
    public string Email { get; set; } = null!;
}