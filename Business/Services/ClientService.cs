using Business.Factories;
using Business.Interfaces;
using Business.Models;
using Data.Interfaces;
using Domain.Models;

namespace Business.Services;
/// Service for handling client-related business logic, including CRUD operations.
/// Utilizes a repository for data access, a factory for mapping between entities and models,
/// and a file storage service for handling uploaded files and avatars.
/// </summary>
public class ClientService(
    IClientRepository clientRepository,
    ClientFactory clientFactory,
    IFileStorageService fileStorageService) : IClientService
{
    private readonly IClientRepository _clientRepository = clientRepository;
    private readonly ClientFactory _clientFactory = clientFactory;
    private readonly IFileStorageService _fileStorageService = fileStorageService;

    private const string ClientUploadsFolder = "clients/clientuploads";
    private const string ClientAvatarFolder = "Clients/avatars";

    /// <summary>
    /// Retrieves all clients and maps them to client models.
    /// </summary>
    public async Task<ServiceResult<IEnumerable<ClientModel>>> GetAllAsync()
    {
        var entities = await _clientRepository.GetAllAsync();
        var models = entities.Select(_clientFactory.Create);
        return ServiceResult<IEnumerable<ClientModel>>.Success(models);
    }

    /// <summary>
    /// Retrieves a single client by its unique identifier.
    /// </summary>
    public async Task<ServiceResult<ClientModel>> GetByIdAsync(int id)
    {
        var entity = await _clientRepository.GetOneAsync(c => c.ClientId == id);
        if (entity == null)
            return ServiceResult<ClientModel>.Failure("Client not found", 404);

        return ServiceResult<ClientModel>.Success(_clientFactory.Create(entity));
    }

    /// <summary>
    /// Creates a new client from the provided form, assigns an avatar if none provided,
    /// and commits the new record in a transactional scope.
    /// </summary>
    public async Task<ServiceResult<ClientModel>> CreateAsync(AddClientForm form, string? imageName)
    {
        var exists = await _clientRepository.ExistsAsync(c => c.ClientEmail == form.ClientEmail);
        if (exists)
            return ServiceResult<ClientModel>.Failure("A client with that email already exists.", 409);

        imageName ??= _fileStorageService.GetRandomAvatar(ClientAvatarFolder);

        var entity = _clientFactory.Create(form, imageName);

        await _clientRepository.BeginTransactionAsync();

        try
        {
            await _clientRepository.CreateAsync(entity);
            await _clientRepository.SaveToDatabaseAsync();
            await _clientRepository.CommitTransactionAsync();

            return ServiceResult<ClientModel>.Success(_clientFactory.Create(entity), 201);
        }
        catch (Exception ex)
        {
            await _clientRepository.RollbackTransactionAsync();
            return ServiceResult<ClientModel>.Failure($"Failed to create client: {ex.Message}", 500);
        }
    }

    /// <summary>
    /// Updates an existing client’s details, handles an optional new image upload,
    /// and persists changes within a transaction.
    /// </summary>
    public async Task<ServiceResult<ClientModel>> UpdateAsync(EditClientForm form)
    {
        var entity = await _clientRepository.GetOneAsync(c => c.ClientId == form.Id);
        if (entity == null)
            return ServiceResult<ClientModel>.Failure("Client not found", 404);

        var emailExists = await _clientRepository.ExistsAsync(c =>
            c.ClientEmail == form.ClientEmail && c.ClientId != form.Id);
        if (emailExists)
            return ServiceResult<ClientModel>.Failure("Another client with this email already exists.", 409);

        if (form.ClientImage is { Length: > 0 })
        {
            string newImageName = await _fileStorageService.SaveFileAsync(form.ClientImage, ClientUploadsFolder);
            form.ImageName = newImageName;
        }

        _clientFactory.Update(entity, form);

        await _clientRepository.BeginTransactionAsync();

        try
        {
            _clientRepository.Update(entity);
            await _clientRepository.SaveToDatabaseAsync();
            await _clientRepository.CommitTransactionAsync();

            return ServiceResult<ClientModel>.Success(_clientFactory.Create(entity));
        }
        catch (Exception ex)
        {
            await _clientRepository.RollbackTransactionAsync();
            return ServiceResult<ClientModel>.Failure($"Failed to update client: {ex.Message}", 500);
        }
    }

    /// <summary>
    /// Deletes the client identified by the given ID from the data store.
    /// </summary>
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

