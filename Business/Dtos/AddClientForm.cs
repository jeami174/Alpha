using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
namespace Business.Dtos;

public class AddClientForm
{
    [Display(Name = "Client Image", Prompt = "Upload a image")]
    [DataType(DataType.Upload)]
    public IFormFile? ClientImage { get; set; }

    [Display(Name = "Client Name", Prompt= "Enter client name")]
    [DataType(DataType.Text)]
    [Required(ErrorMessage = "Required")]
    public string ClientName { get; set; } = null!;

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
