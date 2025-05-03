using Data.Context;
using Data.Entities;
using Data.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Data.Repositories;

/// <summary>
/// Provides data access for <see cref="ProjectEntity"/>, including retrieval
/// with related client and member data, sorting by name or dates, and filtering
/// by creation timestamp.
/// </summary>
public class ProjectRepository(DataContext context) : BaseRepository<ProjectEntity>(context), IProjectRepository
{
    /// <summary>
    /// Retrieves all projects with their client and member relationships,
    /// sorted by project name in ascending or descending order.
    /// </summary>
    public async Task<IEnumerable<ProjectEntity>> GetAllSortedByNameAsync(bool ascending = true)
    {
        var query = _dbSet
            .Include(p => p.Client)
            .Include(p => p.Members);

        return ascending
            ? await query.OrderBy(p => p.ProjectName).ToListAsync()
            : await query.OrderByDescending(p => p.ProjectName).ToListAsync();
    }

    /// <summary>
    /// Retrieves all projects with their client and member relationships,
    /// sorted by either start date or end date, in ascending or descending order.
    /// </summary>
    public async Task<IEnumerable<ProjectEntity>> GetAllSortedByDateAsync(bool sortByStartDate = true, bool ascending = true)
    {
        var query = _dbSet
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

    /// <summary>
    /// Retrieves all projects created after the specified date,
    /// including their client and member relationships.
    /// </summary>
    public async Task<IEnumerable<ProjectEntity>> GetCreatedAfterAsync(DateTime since)
    {
        return await _dbSet
            .Where(p => p.Created > since)
            .Include(p => p.Client)
            .Include(p => p.Members)
            .ToListAsync();
    }
}
