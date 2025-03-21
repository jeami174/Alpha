
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace Business.Models;

public class EditMemberForm
{
    [Display(Name = "Member Image", Prompt = "Upload a image")]
    [DataType(DataType.Upload)]
    public IFormFile? MemberImage { get; set; }

    [Display(Name = "First Name", Prompt = "")]
    [DataType(DataType.Text)]
    [Required(ErrorMessage = "Required")]
    public string FirstName { get; set; } = null!;

    [Display(Name = "Last Name", Prompt = "")]
    [DataType(DataType.Text)]
    [Required(ErrorMessage = "Required")]
    public string LastName { get; set; } = null!;

    [Display(Name = "Email", Prompt = "")]
    [DataType(DataType.EmailAddress)]
    [Required(ErrorMessage = "Required")]
    [RegularExpression(@"^[A-Za-z0-9._%+\-]+@[A-Za-z0-9.\-]+\.[A-Za-z]{2,}$", ErrorMessage = "Invalid email address")]
    public string MemberEmail { get; set; } = null!;

    [Display(Name = "Phone", Prompt = "")]
    [DataType(DataType.PhoneNumber)]
    [Required(ErrorMessage = "Required")]
    public string? Phone { get; set; }

    [Display(Name = "Job Title", Prompt = "")]
    [DataType(DataType.Text)]
    [Required(ErrorMessage = "Required")]
    public string? JobTitle { get; set; }

    [Display(Name = "Address", Prompt = "")]
    [DataType(DataType.Text)]
    public string? Address { get; set; }

    [Display(Name = "Date of Birth", Prompt = "")]
    public DateTime? DateOfBirth { get; set; }
}
