using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace Business.Dtos;

public class AddProjectForm
{
    [Display(Name = "Project Image", Prompt = "Upload a image")]
    [DataType(DataType.Upload)]
    public IFormFile? ProjectImage { get; set; }

    [Display(Name = "Project Name", Prompt = "Project Name")]
    [DataType(DataType.Text)]
    [Required(ErrorMessage = "Required")]
    public string ProjectName { get; set; } = null!;

    /// <summary>
    /// Hämta in lista med befintliga klienter
    /// </summary>

    [Display(Name = "Client Email", Prompt = "Enter client email")]
    [DataType(DataType.EmailAddress)]
    [Required(ErrorMessage = "Required")]
    [RegularExpression(@"^[A-Za-z0-9._%+\-]+@[A-Za-z0-9.\-]+\.[A-Za-z]{2,}$", ErrorMessage = "Invalid email address")]
    public string ClientEmail { get; set; } = null!;

    [Display(Name = "Location", Prompt = "Enter client location")]
    [DataType(DataType.Text)]
    public string? Location { get; set; }

    [Display(Name = "Phone", Prompt = "Enter client phone number")]
    [DataType(DataType.PhoneNumber)]
    public string? Phone { get; set; }
}
