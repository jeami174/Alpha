using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace Business.Models;

public class AddProjectForm
{
    [Display(Name = "Project Image", Prompt = "Upload a image")]
    [DataType(DataType.Upload)]
    public IFormFile? ProjectImage { get; set; }

    [Display(Name = "Project Name", Prompt = "Project Name")]
    [DataType(DataType.Text)]
    [Required(ErrorMessage = "Required")]
    public string ProjectName { get; set; } = null!;

    [Display(Name = "Description", Prompt = "Type something")]
    [DataType(DataType.Text)]
    [Required(ErrorMessage = "Required")]
    public string Description { get; set; } = null!;

    [Display(Name = "Start Date", Prompt = "Select start date")]
    public DateTime? StartDate { get; set; }

    [Display(Name = "End Date", Prompt = "Select end date")]
    public DateTime? EndDate { get; set; }

    [Display(Name = "Budget", Prompt = "Select a budget")]
    public decimal? Budget { get; set; }

}
