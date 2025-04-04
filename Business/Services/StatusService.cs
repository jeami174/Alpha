using Business.Factories;
using Business.Interfaces;
using Business.Models;
using Data.Entities;
using Data.Interfaces;

namespace Business.Services;

public class StatusService : IStatusService
{
    private readonly IStatusRepository _statusRepository;
    private readonly StatusFactory _statusFactory;

    public StatusService(IStatusRepository statusRepository, StatusFactory statusFactory)
    {
        _statusRepository = statusRepository;
        _statusFactory = statusFactory;
    }

    public async Task<ServiceResult<IEnumerable<StatusModel>>> GetAllAsync()
    {
        var entities = await _statusRepository.GetAllAsync();
        var models = entities.Select(_statusFactory.Create);
        return ServiceResult<IEnumerable<StatusModel>>.Success(models);
    }

    public async Task<ServiceResult<StatusModel>> GetByIdAsync(int id)
    {
        var entity = await _statusRepository.GetOneAsync(s => s.Id == id);
        if (entity == null)
            return ServiceResult<StatusModel>.Failure("Status not found", 404);

        var model = _statusFactory.Create(entity);
        return ServiceResult<StatusModel>.Success(model);
    }

    public async Task<ServiceResult<StatusModel>> CreateAsync(StatusFormModel form)
    {
        var exists = await _statusRepository.ExistsAsync(s => s.StatusName == form.StatusName);
        if (exists)
            return ServiceResult<StatusModel>.Failure("A status with the same name already exists.", 409);

        var entity = new StatusEntity { StatusName = form.StatusName };

        try
        {
            await _statusRepository.CreateAsync(entity);
            await _statusRepository.SaveToDatabaseAsync();

            var model = _statusFactory.Create(entity);
            return ServiceResult<StatusModel>.Success(model, 201);
        }
        catch (Exception ex)
        {
            return ServiceResult<StatusModel>.Failure($"Something went wrong: {ex.Message}", 500);
        }
    }

    public async Task<ServiceResult<StatusModel>> UpdateAsync(EditStatusForm form)
    {
        var entity = await _statusRepository.GetOneAsync(s => s.Id == form.Id);
        if (entity == null)
            return ServiceResult<StatusModel>.Failure("Status not found", 404);

        var nameExists = await _statusRepository.ExistsAsync(s => s.StatusName == form.StatusName && s.Id != form.Id);
        if (nameExists)
            return ServiceResult<StatusModel>.Failure("Another status with the same name already exists.", 409);

        entity.StatusName = form.StatusName;

        try
        {
            _statusRepository.Update(entity);
            await _statusRepository.SaveToDatabaseAsync();

            var model = _statusFactory.Create(entity);
            return ServiceResult<StatusModel>.Success(model);
        }
        catch (Exception ex)
        {
            return ServiceResult<StatusModel>.Failure($"Update failed: {ex.Message}", 500);
        }
    }

    public async Task<ServiceResult<bool>> DeleteAsync(int id)
    {
        var entity = await _statusRepository.GetOneAsync(s => s.Id == id);
        if (entity == null)
            return ServiceResult<bool>.Failure("Status not found", 404);

        try
        {
            _statusRepository.Delete(entity);
            await _statusRepository.SaveToDatabaseAsync();
            return ServiceResult<bool>.Success(true, 200);
        }
        catch (Exception ex)
        {
            return ServiceResult<bool>.Failure($"Delete failed: {ex.Message}", 500);
        }
    }
}
