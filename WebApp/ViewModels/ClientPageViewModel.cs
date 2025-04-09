using Business.Models;
using Domain.Models;

namespace Domain.ViewModels;

public class ClientPageViewModel
{
    public IEnumerable<ClientModel> Clients { get; set; } = [];
    public AddClientForm NewClientForm { get; set; } = new();
}
