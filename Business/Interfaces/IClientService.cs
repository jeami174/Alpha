using Business.Models;
using Domain.Models;

namespace Business.Interfaces;

public interface IClientService
{
    Task<ServiceResult<IEnumerable<ClientModel>>> GetAllAsync();
    Task<ServiceResult<ClientModel>> GetByIdAsync(int id);
    Task<ServiceResult<ClientModel>> CreateAsync(AddClientForm form);
    Task<ServiceResult<ClientModel>> UpdateAsync(EditClientForm form);
    Task<ServiceResult<bool>> DeleteAsync(int id);
}
