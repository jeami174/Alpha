using Data.Context;
using System.Diagnostics;
using Data.Interfaces;
using Microsoft.EntityFrameworkCore;
using Data.Entities;
using System.Linq.Expressions;


namespace Data.Repositories;

public class MemberRepository(DataContext context) : BaseRepository<MemberEntity>(context), IMemberRepository
{

    /// <summary>
    /// Söker i förnamn, efternamn eller e-post.
    /// Returnerar en lista med matchande members.
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

    public async Task<MemberEntity?> GetOneWithIncludesAsync(int id)
    {
        return await _dbSet
            .Include(m => m.Role)
            .Include(m => m.Address)
            .FirstOrDefaultAsync(m => m.Id == id);
    }

    public async Task<IEnumerable<MemberEntity>> WhereAsync(Expression<Func<MemberEntity, bool>> predicate)
    {
        return await _dbSet.Where(predicate).ToListAsync();
    }
}

