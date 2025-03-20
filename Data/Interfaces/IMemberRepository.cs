using Data.Entities;

namespace Data.Interfaces;

public interface IMemberRepository : IBaseRepository<MemberEntity>
{
    /// <summary>
    /// Hämtar en lista av Member som matchar ett sökord i för-,
    /// efternamn eller e-post.
    /// </summary>
    Task<IEnumerable<MemberEntity>> SearchMembersAsync(string searchTerm);
}
