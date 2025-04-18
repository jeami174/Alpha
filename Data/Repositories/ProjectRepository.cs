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

    public async Task<IEnumerable<ProjectEntity>> GetAllSortedByNameAsync(bool ascending = true)
    {
        var query = _dbSet
            .Include(p => p.Status)
            .Include(p => p.Client)
            .Include(p => p.Members);

        return ascending
            ? await query.OrderBy(p => p.ProjectName).ToListAsync()
            : await query.OrderByDescending(p => p.ProjectName).ToListAsync();
    }

    public async Task<IEnumerable<ProjectEntity>> GetAllSortedByDateAsync(bool sortByStartDate = true, bool ascending = true)
    {
        var query = _dbSet
            .Include(p => p.Status)
            .Include(p => p.Client)
            .Include(p => p.Members);

        if (sortByStartDate)
            return ascending
                ? await query.OrderBy(p => p.StartDate).ToListAsync()
                : await query.OrderByDescending(p => p.StartDate).ToListAsync();
        else
            return ascending
                ? await query.OrderBy(p => p.EndDate).ToListAsync()
                : await query.OrderByDescending(p => p.EndDate).ToListAsync();
    }
}
