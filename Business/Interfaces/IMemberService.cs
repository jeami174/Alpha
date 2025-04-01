using Business.Models;
using Data.Entities;

namespace Business.Interfaces
{
    public interface IMemberService
    {
        Task AddMemberAsync(AddMemberForm form, string imageName);
        Task UpdateMemberAsync(int id, EditMemberForm form);
        Task DeleteMemberAsync(int id);
        Task<MemberModel?> GetMemberByIdAsync(int id);
        Task<IEnumerable<MemberModel>> GetAllMembersAsync();
        Task<IEnumerable<MemberEntity>> SearchMembersAsync(string searchTerm);
    }
}
