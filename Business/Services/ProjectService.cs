using Business.Factories;
using Business.Interfaces;
using Business.Models;
using Data.Entities;
using Data.Interfaces;
using Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace Business.Services;

public class ProjectService : IProjectService
{
    private readonly IProjectRepository _projectRepository;
    private readonly IClientRepository _clientRepository;
    private readonly IStatusRepository _statusRepository;
    private readonly IMemberRepository _memberRepository;
    private readonly ProjectFactory _projectFactory;

    public ProjectService(
        IProjectRepository projectRepository,
        IClientRepository clientRepository,
        IStatusRepository statusRepository,
        IMemberRepository memberRepository,
        ProjectFactory projectFactory)
    {
        _projectRepository = projectRepository;
        _clientRepository = clientRepository;
        _statusRepository = statusRepository;
        _memberRepository = memberRepository;
        _projectFactory = projectFactory;
    }

    // CREATE
    public async Task<ServiceResult<bool>> CreateAsync(
    AddProjectForm form,
    int clientId,
    int statusId,
    List<int> memberIds,
    string? imageName = null)
    {
        if (form is null || string.IsNullOrWhiteSpace(form.ProjectName))
            return ServiceResult<bool>.Failure("Missing required fields.");

        var client = await _clientRepository.GetOneAsync(c => c.ClientId == clientId);
        var status = await _statusRepository.GetOneAsync(s => s.Id == statusId);
        var members = await _memberRepository.GetAllAsync();

        if (client is null || status is null)
            return ServiceResult<bool>.Failure("Invalid client or status.");

        var selectedMembers = members.Where(m => memberIds.Contains(m.Id)).ToList();

        var project = new ProjectEntity
        {
            ProjectName = form.ProjectName,
            Description = form.Description,
            StartDate = form.StartDate ?? DateTime.Now,
            EndDate = form.EndDate,
            Budget = form.Budget,
            Created = DateTime.Now,
            ImageName = imageName,
            Client = client,
            Status = status,
            Members = selectedMembers
        };

        await _projectRepository.CreateAsync(project);
        await _projectRepository.SaveToDatabaseAsync();

        return ServiceResult<bool>.Success(true, 201);
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

    // UPDATE
    public async Task<ServiceResult<bool>> UpdateProjectAsync(string id, AddProjectForm form, int statusId, int clientId, List<int> memberIds)
    {
        var project = await _projectRepository.GetOneWithDetailsAsync(
            q => q.Include(p => p.Members),
            p => p.Id == id);

        if (project == null)
            return ServiceResult<bool>.Failure("Project not found.", 404);

        var client = await _clientRepository.GetOneAsync(c => c.ClientId == clientId);
        var status = await _statusRepository.GetOneAsync(s => s.Id == statusId);
        var allMembers = await _memberRepository.GetAllAsync();

        if (client is null || status is null)
            return ServiceResult<bool>.Failure("Client or status not found.");

        var selectedMembers = allMembers.Where(m => memberIds.Contains(m.Id)).ToList();

        project.ProjectName = form.ProjectName;
        project.Description = form.Description;
        project.StartDate = form.StartDate ?? DateTime.Now;
        project.EndDate = form.EndDate;
        project.Budget = form.Budget;
        project.Status = status;
        project.Client = client;
        project.Members = selectedMembers;

        _projectRepository.Update(project);
        await _projectRepository.SaveToDatabaseAsync();

        return ServiceResult<bool>.Success(true);
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
