using Business.Models;
using Data.Entities;
using Domain.Models;

namespace Business.Factories;

public class ClientFactory
{
    public ClientModel Create(ClientEntity entity)
    {
        return new ClientModel
        {
            ClientId = entity.ClientId,
            ClientName = entity.ClientName,
            ClientEmail = entity.ClientEmail,
            Location = entity.Location,
            Phone = entity.Phone
        };
    }

    public ClientEntity Create(AddClientForm form)
    {
        return new ClientEntity
        {
            ClientName = form.ClientName,
            ClientEmail = form.ClientEmail,
            Location = form.Location,
            Phone = form.Phone
        };
    }

    public ClientEntity Update(ClientEntity entity, EditClientForm form)
    {
        entity.ClientName = form.ClientName;
        entity.ClientEmail = form.ClientEmail;
        entity.Location = form.Location;
        entity.Phone = form.Phone;

        return entity;
    }
}
