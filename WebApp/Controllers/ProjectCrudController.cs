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
        var editForm = new EditProjectForm
        {
            Id = project.Id,
            ProjectName = project.ProjectName,
            Description = project.Description,
            StartDate = project.StartDate,
            EndDate = project.EndDate,
            Budget = project.Budget,
            ImageName = project.ImageName
        };

        return PartialView("~/Views/Shared/Partials/Project/_EditProjectForm.cshtml", editForm);
    }

    [HttpPost("edit")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditProject([FromForm] EditProjectFormData data)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState
                .Where(x => x.Value?.Errors?.Count > 0)
                .ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value?.Errors?.Select(e => e.ErrorMessage).ToArray()
                    ?? Array.Empty<string>());

            return BadRequest(new { success = false, errors });
        }

        var newImageName = data.Form.ProjectImage is { Length: > 0 }
            ? await _fileStorageService.SaveFileAsync(data.Form.ProjectImage, ProjectUploadsFolder)
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
