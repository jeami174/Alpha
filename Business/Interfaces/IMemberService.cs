using Business.Models;
using Domain.Models;

namespace Business.Interfaces;

public interface IMemberService
{
    Task<ServiceResult<IEnumerable<MemberModel>>> GetAllMembersAsync();
    Task<ServiceResult<MemberModel>> GetMemberByIdAsync(int id);
    Task<ServiceResult<IEnumerable<MemberModel>>> SearchMembersAsync(string searchTerm);
    Task<ServiceResult<MemberModel>> AddMemberAsync(AddMemberForm form, string imageName);
    Task<ServiceResult<MemberModel>> UpdateMemberAsync(int id, EditMemberForm form);
    Task<ServiceResult<bool>> DeleteMemberAsync(int id);
}

