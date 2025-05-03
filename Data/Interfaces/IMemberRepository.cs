using System.Linq.Expressions;
using Data.Entities;

namespace Data.Interfaces;
public interface IMemberRepository : IBaseRepository<MemberEntity>
{
    Task<IEnumerable<MemberEntity>> SearchMembersAsync(string searchTerm);
    Task<MemberEntity?> GetOneWithIncludesAsync(int id);
    Task<IEnumerable<MemberEntity>> WhereAsync(Expression<Func<MemberEntity, bool>> predicate);
    Task<IEnumerable<MemberEntity>> GetCreatedAfterAsync(DateTime since);
}
