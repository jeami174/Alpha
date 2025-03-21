using Business.Models;
using Data.Entities;

namespace Business.Interfaces;

public interface IMemberService
{
    Task AddMemberAsync(AddMemberForm form);

    Task UpdateMemberAsync(int id, EditMemberForm form);

    Task DeleteMemberAsync(int id);

    Task<MemberEntity?> GetMemberByIdAsync(int id);

    Task<IEnumerable<MemberEntity>> GetAllMembersAsync();

    Task<IEnumerable<MemberEntity>> SearchMembersAsync(string searchTerm);
}
