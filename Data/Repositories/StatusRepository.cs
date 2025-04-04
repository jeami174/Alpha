using Data.Context;
using Data.Entities;
using Data.Interfaces;

namespace Data.Repositories;

public class StatusRepository(DataContext context) : BaseRepository<StatusEntity>(context), IStatusRepository
{

}
