using System.ComponentModel.DataAnnotations;

namespace Business.Models;

public class SignUpFormModel
{
    [Display(Name = "First Name", Prompt = "Your first name")]
    [Required(ErrorMessage = "You must enter your first name")]
    public string FirstName { get; set; } = null!;

    [Display(Name = "Last Name", Prompt = "Your last name")]
    [Required(ErrorMessage = "You must enter your last name")]
    public string LastName { get; set; } = null!;

    [Display(Name = "Email", Prompt = "Your email address")]
    [DataType(DataType.EmailAddress)]
    [Required(ErrorMessage = "You must enter your email adress.")]
    [RegularExpression(@"^[A-Za-z0-9._%+\-]+@[A-Za-z0-9.\-]+\.[A-Za-z]{2,}$", ErrorMessage = "Invalid email address")]
    public string Email { get; set; } = null!;

    [Display(Name = "Password", Prompt = "Enter your password")]
    [DataType(DataType.Password)]
    [Required(ErrorMessage = "You must enter a strong password")]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[\W_]).{8,}$", ErrorMessage = "Your password must be at least 8 characters long and include one uppercase letter, one lowercase letter, one number, and one special character.")]
    public string Password { get; set; } = null!;

    [Display(Name = "Confirm Password", Prompt = "Confirm your password")]
    [DataType(DataType.Password)]
    [Required(ErrorMessage = "You must confirm your password.")]
    [Compare(nameof(Password), ErrorMessage = "Passwords do not match")]
    public string ConfirmPassword { get; set; } = null!;

    [Display(Name = "Accept Terms")]
    [Required(ErrorMessage = "You must accept the terms and conditions")]
    public bool AcceptTerms { get; set; }
}
