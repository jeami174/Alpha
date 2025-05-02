using System.ComponentModel.DataAnnotations.Schema;
using Business.Models;
using Domain.Models;

namespace WebApp.ViewModels;

public class AddProjectFormViewModel
{
    public AddProjectFormData FormData { get; set; } = new();
    public IEnumerable<ClientModel> Clients { get; set; } = [];
    public IEnumerable<MemberModel> Members { get; set; } = [];

}
