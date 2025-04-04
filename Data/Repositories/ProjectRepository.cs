using Data.Context;
using Data.Entities;
using Data.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Data.Repositories;

public class ProjectRepository(DataContext context) : BaseRepository<ProjectEntity>(context), IProjectRepository
{
    public async Task<IEnumerable<ProjectEntity>> GetByStatusAsync(string statusName)
    {
        if (string.IsNullOrWhiteSpace(statusName))
            return [];

        return await _dbSet
            .Include(p => p.Status)
            .Include(p => p.Client)
            .Include(p => p.Members)
            .Where(p => p.Status != null && p.Status.StatusName == statusName)
            .ToListAsync();
    }
}
