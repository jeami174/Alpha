using Business.Models;

namespace Business.Interfaces;

public interface IStatusService
{
    Task<ServiceResult<IEnumerable<StatusModel>>> GetAllAsync();
    Task<ServiceResult<StatusModel>> GetByIdAsync(int id);
    Task<ServiceResult<StatusModel>> CreateAsync(StatusFormModel form);
    Task<ServiceResult<StatusModel>> UpdateAsync(EditStatusForm form);
    Task<ServiceResult<bool>> DeleteAsync(int id);
}
