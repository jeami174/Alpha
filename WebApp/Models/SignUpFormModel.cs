using System.ComponentModel.DataAnnotations;

namespace WebApp.Models
{
    public class SignUpFormModel
    {
        [Display(Name = "Full Name", Prompt = "Your full name")]
        [Required(ErrorMessage = "Required")]
        public string FullName { get; set; } = null!;

        [Display(Name = "Email", Prompt = "Your email address")]
        [DataType(DataType.EmailAddress)]
        [Required(ErrorMessage = "Required")]
        [RegularExpression(@"^[A-Za-z0-9._%+\-]+@[A-Za-z0-9.\-]+\.[A-Za-z]{2,}$", ErrorMessage = "Invalid email address")]
        public string Email { get; set; } = null!;

        [Display(Name = "Password", Prompt = "Enter your password")]
        [DataType(DataType.Password)]
        [Required(ErrorMessage = "Required")]
        public string Password { get; set; } = null!;

        [Display(Name = "Confirm Password", Prompt = "Confirm your password")]
        [DataType(DataType.Password)]
        [Required(ErrorMessage = "Required")]
        [Compare("Password", ErrorMessage = "Passwords do not match")]
        public string ConfirmPassword { get; set; } = null!;

        [Display(Name = "Accept Terms")]
        [Required(ErrorMessage = "You must accept the terms and conditions")]
        public bool AcceptTerms { get; set; }
    }
}
