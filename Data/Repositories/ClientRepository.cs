using Data.Context;
using Data.Entities;
using Data.Interfaces;

namespace Data.Repositories;

public class ClientRepository(DataContext context) : BaseRepository<ClientEntity>(context), IClientRepository
{
}
