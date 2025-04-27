using Business.Interfaces;
using Business.Models;
using Domain.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApp.ViewModels;

namespace WebApp.Controllers;

[Authorize(Policy = "Admins")]
[Route("projectcrud")]
public class ProjectCrudController : Controller
{
    private readonly IProjectService _projectService;
    private readonly IStatusService _statusService;
    private readonly IClientService _clientService;
    private readonly IMemberService _memberService;
    private readonly IFileStorageService _fileStorageService;

    private const string ProjectUploadsFolder = "projects/projectuploads";
    private const string AvatarsFolder = "projects/avatars";

    public ProjectCrudController(
        IProjectService projectService,
        IStatusService statusService,
        IClientService clientService,
        IMemberService memberService,
        IFileStorageService fileStorageService)
    {
        _projectService = projectService;
        _statusService = statusService;
        _clientService = clientService;
        _memberService = memberService;
        _fileStorageService = fileStorageService;
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddProject([FromForm] AddProjectFormViewModel vm)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState
                .Where(x => x.Value?.Errors?.Count > 0)
                .ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value?.Errors.Select(e => e.ErrorMessage).ToArray()
                            ?? Array.Empty<string>()
                );
            return BadRequest(new { success = false, errors });
        }

        var data = vm.FormData;

        var imageName = data.Form.ProjectImage is { Length: > 0 }
            ? await _fileStorageService.SaveFileAsync(data.Form.ProjectImage, ProjectUploadsFolder)
            : _fileStorageService.GetRandomAvatar(AvatarsFolder);

        // Anropa tjänsten –  skickar null för status just nu
        var result = await _projectService.CreateAsync(
            data.Form,
            data.SelectedClientId,
            0,                     // status avaktiverad tills vidare
            data.SelectedMemberIds,
            imageName
        );

        if (!result.Succeeded)
            return BadRequest(new { success = false, error = result.Error ?? "Failed to create project." });

        return Ok(new { redirectUrl = Url.Action("Projects", "Projects") });
    }


    [HttpGet("edit/{id}")]
    public async Task<IActionResult> EditProject(string id)
    {
        var projectResult = await _projectService.GetProjectByIdAsync(id);
        if (!projectResult.Succeeded || projectResult.Result == null)
            return NotFound();

        var project = projectResult.Result;

        var clientsResult = await _clientService.GetAllAsync();
        var statusesResult = await _statusService.GetAllAsync();
        var membersResult = await _memberService.GetAllMembersAsync();

        var allMembers = (membersResult.Result ?? Enumerable.Empty<MemberModel>())
            .UnionBy(project.MemberModels, m => m.Id)
            .ToList();

        var vm = new EditProjectFormViewModel
        {
            FormData = new EditProjectFormData
            {
                Form = new EditProjectForm
                {
                    Id = project.Id,
                    ImageName = project.ImageName,
                    ProjectName = project.ProjectName,
                    Description = project.Description ?? string.Empty,
                    StartDate = project.StartDate,
                    EndDate = project.EndDate,
                    Budget = project.Budget
                },
                SelectedClientId = project.ClientModel.ClientId,
                SelectedStatusId = project.StatusModel.Id,
                SelectedMemberIds = project.MemberModels.Select(m => m.Id).ToList()
            },
            Clients = clientsResult.Result ?? Enumerable.Empty<ClientModel>(),
            Statuses = statusesResult.Result ?? Enumerable.Empty<StatusModel>(),
            Members = allMembers
        };

        vm.FormData.SelectedMemberIdsRaw = string.Join(",", vm.FormData.SelectedMemberIds);

        return PartialView("~/Views/Shared/Partials/Sections/_EditProjectForm.cshtml", vm);
    }


    [HttpPost("edit")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditProject(EditProjectFormViewModel vm)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState
                .Where(x => x.Value?.Errors.Count > 0)
                .ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value?.Errors.Select(e => e.ErrorMessage).ToArray()
                );
            return BadRequest(new { success = false, errors });
        }

        var data = vm.FormData;

        data.SelectedMemberIds = (data.SelectedMemberIdsRaw ?? "")
            .Split(",", StringSplitOptions.RemoveEmptyEntries)
            .Where(id => int.TryParse(id, out _)) //Nytt Jeanette
            .Select(id => int.Parse(id))
            .ToList();

        var newImageName = data.Form.ProjectImage is { Length: > 0 }
            ? await _fileStorageService.SaveFileAsync(
                  data.Form.ProjectImage, ProjectUploadsFolder)
            : data.Form.ImageName;

        var result = await _projectService.UpdateProjectAsync(
            data.Form,
            data.SelectedClientId,
            data.SelectedStatusId,
            data.SelectedMemberIds,
            newImageName
        );

        return result.Succeeded
            ? Ok(new { success = true })
            : BadRequest(new { success = false, error = result.Error });
    }

    [HttpPost("delete/{id}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteProject(string id)
    {
        var result = await _projectService.DeleteProjectAsync(id);
        return result.Succeeded
            ? Ok(new { success = true })
            : BadRequest(new { success = false, error = result.Error });
    }
}