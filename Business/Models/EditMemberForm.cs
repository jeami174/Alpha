
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace Business.Models;

public class EditMemberForm
{
    public int Id { get; set; }

    public string? ImageName { get; set; }

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

    [Display(Name = "Date of Birth", Prompt = "")]
    public DateTime? DateOfBirth { get; set; }

    [Display(Name = "Job Title", Prompt = "Enter job title")]
    [DataType(DataType.Text)]
    [Required(ErrorMessage = "Required")]
    public string? RoleName { get; set; }

    [Display(Name = "Street", Prompt = "Enter Street")]
    [DataType(DataType.Text)]
    public string? Street { get; set; }

    [Display(Name = "PostalCode", Prompt = "Enter Postal code")]
    [DataType(DataType.Text)]
    public string? PostalCode { get; set; }

    [Display(Name = "City", Prompt = "Enter City")]
    [DataType(DataType.Text)]
    public string? City { get; set; }

}
