﻿using Business.Factories;
using Business.Interfaces;
using Business.Models;
using Data.Entities;
using Data.Interfaces;
using Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace Business.Services;

/// <summary>
/// Handles project-related business operations, including creation, retrieval,
/// updating, deletion, and fetching of newly created projects with related data.
/// </summary>
public class ProjectService(
    IProjectRepository projectRepository,
    IClientRepository clientRepository,
    IMemberRepository memberRepository,
    ProjectFactory projectFactory) : IProjectService
{
    private readonly IProjectRepository _projectRepository = projectRepository;
    private readonly IClientRepository _clientRepository = clientRepository;
    private readonly IMemberRepository _memberRepository = memberRepository;
    private readonly ProjectFactory _projectFactory = projectFactory;

    /// <summary>
    /// Creates a new project for the specified client, assigns members and an image,
    /// and persists the record within a transaction.
    /// </summary>
    public async Task<ServiceResult<ProjectModel>> CreateAsync(
        AddProjectForm form,
        int clientId,
        List<int> memberIds,
        string imageName)
    {
        await _projectRepository.BeginTransactionAsync();

        try
        {
            var client = await _clientRepository.GetOneAsync(c => c.ClientId == clientId);

            if (client is null)
                return ServiceResult<ProjectModel>.Failure("Client not found", 400);

            var selectedMembers = await _memberRepository
                .WhereAsync(m => memberIds.Contains(m.Id));

            var project = _projectFactory.Create(
                form,
                client,
                selectedMembers.ToList(),
                imageName
            );

            project.ImageName = imageName;

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

    /// <summary>
    /// Retrieves all projects, including related client and members,
    /// with optional sorting by name, creation date, start date, or end date.
    /// </summary>
    public async Task<ServiceResult<IEnumerable<ProjectModel>>> GetAllProjectsAsync(string? sortBy = null)
    {
        var projects = await _projectRepository.GetAllWithDetailsAsync(q =>
            q.Include(p => p.Client)
             .Include(p => p.Members));


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

    /// <summary>
    /// Retrieves a single project by its identifier, including its client and members.
    /// </summary>
    public async Task<ServiceResult<ProjectModel>> GetProjectByIdAsync(string id)
    {
        var project = await _projectRepository.GetOneWithDetailsAsync(
            q => q.Include(p => p.Client)
                  .Include(p => p.Members),
            p => p.Id == id);

        if (project == null)
            return ServiceResult<ProjectModel>.Failure($"Project with id {id} was not found.", 404);

        return ServiceResult<ProjectModel>.Success(_projectFactory.Create(project));
    }

    /// <summary>
    /// Updates an existing project’s details, members, client association,
    /// and optional image, within a transaction.
    /// </summary>
    public async Task<ServiceResult<ProjectModel>> UpdateProjectAsync(
    EditProjectForm form,
    int clientId,
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

            if (client == null)
                return ServiceResult<ProjectModel>.Failure("Client not found", 400);

            var selectedMembers = await _memberRepository
                .WhereAsync(m => memberIds.Contains(m.Id));

            _projectFactory.Update(project, form, client, selectedMembers.ToList());

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

    /// <summary>
    /// Deletes a project by its identifier.
    /// </summary>
    public async Task<ServiceResult<bool>> DeleteProjectAsync(string id)
    {
        var project = await _projectRepository.GetOneAsync(p => p.Id == id);
        if (project == null)
            return ServiceResult<bool>.Failure("Project not found.", 404);

        _projectRepository.Delete(project);
        await _projectRepository.SaveToDatabaseAsync();

        return ServiceResult<bool>.Success(true);
    }

    /// <summary>
    /// Help-method
    /// Retrieves projects created after the specified date, for notification purposes.
    /// </summary>
    public async Task<IEnumerable<ProjectModel>> GetNewProjectsAsync(DateTime since)
    {
        var projects = await _projectRepository.GetCreatedAfterAsync(since);

        var models = projects.Select(p => _projectFactory.Create(p));
        return models;
    }
}
