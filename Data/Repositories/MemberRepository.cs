using Data.Context;
using System.Diagnostics;
using Data.Interfaces;
using Microsoft.EntityFrameworkCore;
using Data.Entities;
using System.Data.Entity;

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
}

