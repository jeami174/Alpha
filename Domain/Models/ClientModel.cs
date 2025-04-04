namespace Domain.Models;

public class ClientModel
{
    public int ClientId { get; set; }

    public string ClientName { get; set; } = null!;

    public string ClientEmail { get; set; } = null!;

    public string? Location { get; set; }

    public string? Phone { get; set; }

}
