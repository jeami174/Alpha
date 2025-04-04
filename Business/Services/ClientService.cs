using Business.Factories;
using Business.Interfaces;
using Business.Models;
using Data.Interfaces;
using Domain.Models;

namespace Business.Services;

public class ClientService(IClientRepository clientRepository, ClientFactory clientFactory) : IClientService
{
    private readonly IClientRepository _clientRepository = clientRepository;
    private readonly ClientFactory _clientFactory = clientFactory;

    public async Task<ServiceResult<IEnumerable<ClientModel>>> GetAllAsync()
    {
        var entities = await _clientRepository.GetAllAsync();
        var models = entities.Select(_clientFactory.Create);
        return ServiceResult<IEnumerable<ClientModel>>.Success(models);
    }

    public async Task<ServiceResult<ClientModel>> GetByIdAsync(int id)
    {
        var entity = await _clientRepository.GetOneAsync(c => c.ClientId == id);
        if (entity == null)
            return ServiceResult<ClientModel>.Failure("Client not found", 404);

        return ServiceResult<ClientModel>.Success(_clientFactory.Create(entity));
    }

    public async Task<ServiceResult<ClientModel>> CreateAsync(AddClientForm form)
    {
        var exists = await _clientRepository.ExistsAsync(c => c.ClientEmail == form.ClientEmail);
        if (exists)
            return ServiceResult<ClientModel>.Failure("A client with that email already exists.", 409);

        var entity = _clientFactory.Create(form);

        try
        {
            await _clientRepository.CreateAsync(entity);
            await _clientRepository.SaveToDatabaseAsync();

            return ServiceResult<ClientModel>.Success(_clientFactory.Create(entity), 201);
        }
        catch (Exception ex)
        {
            return ServiceResult<ClientModel>.Failure($"Failed to create client: {ex.Message}", 500);
        }
    }

    public async Task<ServiceResult<ClientModel>> UpdateAsync(EditClientForm form)
    {
        var entity = await _clientRepository.GetOneAsync(c => c.ClientId == form.Id);
        if (entity == null)
            return ServiceResult<ClientModel>.Failure("Client not found", 404);

        var emailExists = await _clientRepository.ExistsAsync(c =>
            c.ClientEmail == form.ClientEmail && c.ClientId != form.Id);

        if (emailExists)
            return ServiceResult<ClientModel>.Failure("Another client with this email already exists.", 409);

        _clientFactory.Update(entity, form);

        try
        {
            _clientRepository.Update(entity);
            await _clientRepository.SaveToDatabaseAsync();
            return ServiceResult<ClientModel>.Success(_clientFactory.Create(entity));
        }
        catch (Exception ex)
        {
            return ServiceResult<ClientModel>.Failure($"Failed to update client: {ex.Message}", 500);
        }
    }

    public async Task<ServiceResult<bool>> DeleteAsync(int id)
    {
        var entity = await _clientRepository.GetOneAsync(c => c.ClientId == id);
        if (entity == null)
            return ServiceResult<bool>.Failure("Client not found", 404);

        try
        {
            _clientRepository.Delete(entity);
            await _clientRepository.SaveToDatabaseAsync();
            return ServiceResult<bool>.Success(true);
        }
        catch (Exception ex)
        {
            return ServiceResult<bool>.Failure($"Failed to delete client: {ex.Message}", 500);
        }
    }
}
