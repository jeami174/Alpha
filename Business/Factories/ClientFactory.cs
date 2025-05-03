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
            Phone = entity.Phone,
            ImageName = string.IsNullOrWhiteSpace(entity.ImageName)
                ? "uploads/Clients/avatars/default.svg"
                : entity.ImageName.Replace("\\", "/")
        };
    }
    public ClientEntity Create(AddClientForm form, string imageName)
    {
        return new ClientEntity
        {
            ClientName = form.ClientName,
            ClientEmail = form.ClientEmail,
            Location = form.Location,
            Phone = form.Phone,
            ImageName = imageName
        };
    }
    public ClientEntity Update(ClientEntity entity, EditClientForm form)
    {
        entity.ClientName = form.ClientName;
        entity.ClientEmail = form.ClientEmail;
        entity.Location = form.Location;
        entity.Phone = form.Phone;

        if (!string.IsNullOrEmpty(form.ImageName))
        {
            entity.ImageName = form.ImageName;
        }

        return entity;
    }
}
