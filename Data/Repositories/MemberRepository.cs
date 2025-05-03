using Data.Context;
using System.Diagnostics;
using Data.Interfaces;
using Microsoft.EntityFrameworkCore;
using Data.Entities;
using System.Linq.Expressions;

namespace Data.Repositories;

/// <summary>
/// Repository for MemberEntity providing common CRUD operations
/// plus member-specific queries like search and filtering by creation date.
/// </summary>
public class MemberRepository(DataContext context) : BaseRepository<MemberEntity>(context), IMemberRepository
{

    /// <summary>
    /// Searches members by first name, last name, or email (case-insensitive).
    /// Returns all members when the search term is null or whitespace.
    /// </summary>
    public async Task<IEnumerable<MemberEntity>> SearchMembersAsync(string searchTerm)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                
                return await GetAllAsync();
            }

            searchTerm = searchTerm.Trim().ToLower();

            return await _dbSet
                .Where(m => m.FirstName.ToLower().Contains(searchTerm)
                         || m.LastName.ToLower().Contains(searchTerm)
                         || m.Email.ToLower().Contains(searchTerm))
                .ToListAsync();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error searching members: {ex.Message}");
            return Enumerable.Empty<MemberEntity>();
        }
    }

    /// <summary>
    /// Retrieves a single member by ID, including related Role and Address entities.
    /// </summary>
    public async Task<MemberEntity?> GetOneWithIncludesAsync(int id)
    {
        return await _dbSet
            .Include(m => m.Role)
            .Include(m => m.Address)
            .FirstOrDefaultAsync(m => m.Id == id);
    }

    /// <summary>
    /// Retrieves members matching the specified predicate.
    /// </summary>
    public async Task<IEnumerable<MemberEntity>> WhereAsync(Expression<Func<MemberEntity, bool>> predicate)
    {
        return await _dbSet.Where(predicate).ToListAsync();
    }

    /// <summary>
    /// Retrieves members created after the given date.
    /// </summary>
    public async Task<IEnumerable<MemberEntity>> GetCreatedAfterAsync(DateTime since)
    {
        return await _dbSet
            .Where(m => m.Created > since)
            .ToListAsync();
    }
}

