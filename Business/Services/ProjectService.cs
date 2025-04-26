using Business.Factories;
using Business.Interfaces;
using Business.Models;
using Data.Entities;
using Data.Interfaces;
using Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace Business.Services;

public class ProjectService(
    IProjectRepository projectRepository,
    IClientRepository clientRepository,
    IStatusRepository statusRepository,
    IMemberRepository memberRepository,
    ProjectFactory projectFactory) : IProjectService
{
    private readonly IProjectRepository _projectRepository = projectRepository;
    private readonly IClientRepository _clientRepository = clientRepository;
    private readonly IStatusRepository _statusRepository = statusRepository;
    private readonly IMemberRepository _memberRepository = memberRepository;
    private readonly ProjectFactory _projectFactory = projectFactory;

    public async Task<ServiceResult<ProjectModel>> CreateAsync(
        AddProjectForm form,
        int clientId,
        int statusId,
        List<int> memberIds,
        string imageName)
    {
        await _projectRepository.BeginTransactionAsync();

        try
        {
            var client = await _clientRepository.GetOneAsync(c => c.ClientId == clientId);

            if (client is null)
                return ServiceResult<ProjectModel>.Failure("Client not found", 400);

            // 🚀 Ändra här:
            var selectedMembers = await _memberRepository
                .WhereAsync(m => memberIds.Contains(m.Id));

            var project = _projectFactory.Create(
                form,
                client,
                selectedMembers.ToList(), // 👈 här är de korrekt attached
                imageName
            );

            project.ImageName = imageName;

            // TODO: hämta status från DB
            project.StatusId = 1;

            await _projectRepository.CreateAsync(project);
            await _projectRepository.SaveToDatabaseAsync();
            await _projectRepository.CommitTransactionAsync();

            var model = _projectFactory.Create(project);
            return ServiceResult<ProjectModel>.Success(model, 201);
        }
        catch (Exception ex)
        {
            await _projectRepository.RollbackTransactionAsync();
            return ServiceResult<ProjectModel>.Failure($"Failed to create project: {ex.Message}", 500);
        }
    }
    // READ ALL
    public async Task<ServiceResult<IEnumerable<ProjectModel>>> GetAllProjectsAsync(string? sortBy = null, string? statusFilter = null)
    {
        var projects = await _projectRepository.GetAllWithDetailsAsync(q =>
            q.Include(p => p.Client)
             .Include(p => p.Status)
             .Include(p => p.Members));

        if (!string.IsNullOrWhiteSpace(statusFilter))
        {
            projects = projects.Where(p => p.Status?.StatusName.ToLower() == statusFilter.ToLower());
        }

        projects = sortBy switch
        {
            "name" => projects.OrderBy(p => p.ProjectName),
            "created" => projects.OrderByDescending(p => p.Created),
            "start" => projects.OrderBy(p => p.StartDate),
            "end" => projects.OrderBy(p => p.EndDate),
            _ => projects
        };

        var models = projects.Select(p => _projectFactory.Create(p));
        return ServiceResult<IEnumerable<ProjectModel>>.Success(models);
    }

    // READ SINGLE
    public async Task<ServiceResult<ProjectModel>> GetProjectByIdAsync(string id)
    {
        var project = await _projectRepository.GetOneWithDetailsAsync(
            q => q.Include(p => p.Client)
                  .Include(p => p.Status)
                  .Include(p => p.Members),
            p => p.Id == id);

        if (project == null)
            return ServiceResult<ProjectModel>.Failure($"Project with id {id} was not found.", 404);

        return ServiceResult<ProjectModel>.Success(_projectFactory.Create(project));
    }

    public async Task<ServiceResult<ProjectModel>> UpdateProjectAsync(
    EditProjectForm form,
    int clientId,
    int statusId,
    List<int> memberIds,
    string? newImageName)
    {
        var project = await _projectRepository.GetOneWithDetailsAsync(
            q => q.Include(p => p.Members),
            p => p.Id == form.Id);

        if (project == null)
            return ServiceResult<ProjectModel>.Failure("Project not found", 404);

        await _projectRepository.BeginTransactionAsync();

        try
        {
            var client = await _clientRepository.GetOneAsync(c => c.ClientId == clientId);
            var status = await _statusRepository.GetOneAsync(s => s.Id == statusId);

            if (client == null || status == null)
                return ServiceResult<ProjectModel>.Failure("Client or status not found", 400);

            // 🚀 Ändra här:
            var selectedMembers = await _memberRepository
                .WhereAsync(m => memberIds.Contains(m.Id));

            _projectFactory.Update(project, form, client, status, selectedMembers.ToList());

            project.ImageName = newImageName ?? form.ImageName;

            _projectRepository.Update(project);
            await _projectRepository.SaveToDatabaseAsync();
            await _projectRepository.CommitTransactionAsync();

            var model = _projectFactory.Create(project);
            return ServiceResult<ProjectModel>.Success(model);
        }
        catch (Exception ex)
        {
            await _projectRepository.RollbackTransactionAsync();
            return ServiceResult<ProjectModel>.Failure($"Failed to update project: {ex.Message}", 500);
        }
    }


    // DELETE
    public async Task<ServiceResult<bool>> DeleteProjectAsync(string id)
    {
        var project = await _projectRepository.GetOneAsync(p => p.Id == id);
        if (project == null)
            return ServiceResult<bool>.Failure("Project not found.", 404);

        _projectRepository.Delete(project);
        await _projectRepository.SaveToDatabaseAsync();

        return ServiceResult<bool>.Success(true);
    }
}
