using System.Linq.Expressions; // behövs för Expression
using Data.Entities;

namespace Data.Interfaces;

public interface IMemberRepository : IBaseRepository<MemberEntity>
{
    /// <summary>
    /// Hämtar en lista av Member som matchar ett sökord i för-,
    /// efternamn eller e-post.
    /// </summary>
    Task<IEnumerable<MemberEntity>> SearchMembersAsync(string searchTerm);

    /// <summary>
    /// Hämtar en enskild member med tillhörande relationer (t.ex. Address och Role).
    /// </summary>
    Task<MemberEntity?> GetOneWithIncludesAsync(int id);

    /// <summary>
    /// Hämtar en lista av Member baserat på ett fritt predicate (exempelvis en lista med id:n).
    /// Dessa entities är trackade av DbContext.
    /// </summary>
    Task<IEnumerable<MemberEntity>> WhereAsync(Expression<Func<MemberEntity, bool>> predicate);

    /// <summary>
    /// Hämtar alla members som skapats efter ett visst datum.
    /// Används t.ex. vid notifieringar.
    /// </summary>
    Task<IEnumerable<MemberEntity>> GetCreatedAfterAsync(DateTime since);

}
