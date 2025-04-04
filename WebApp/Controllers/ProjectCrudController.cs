using Business.Interfaces;
using Business.Models;
using Microsoft.AspNetCore.Mvc;
using WebApp.Helpers;
using WebApp.ViewModels;

namespace WebApp.Controllers;

public class ProjectCrudController(IProjectService projectService, IStatusService statusService, IClientService clientService, IMemberService memberService, IFileStorageService fileStorageService) : Controller
{
    private readonly IProjectService _projectService = projectService;
    private readonly IFileStorageService _fileStorageService = fileStorageService;
    private readonly IStatusService _statusService = statusService;
    private readonly IClientService _clientService = clientService;
    private readonly IMemberService _memberService = memberService;

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        var statuses = await _statusService.GetAllAsync();
        var clients = await _clientService.GetAllAsync();
        var members = await _memberService.GetAllMembersAsync();

        var viewModel = new ProjectFormViewModel
        {
            Statuses = statuses.Result?.ToList() ?? [],
            Clients = clients.Result?.ToList() ?? [],
            Members = members.Result?.ToList() ?? []
        };

        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddProject([FromForm] AddProjectFormData data)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState
                .Where(x => x.Value?.Errors.Count > 0)
                .ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value?.Errors.Select(x => x.ErrorMessage).ToArray()
                );

            return BadRequest(new { success = false, errors });
        }

        string? imageName = data.Form.ProjectImage is { Length: > 0 }
            ? await _fileStorageService.SaveFileAsync(data.Form.ProjectImage, "projectuploads")
            : null;

        var result = await _projectService.CreateAsync(
            data.Form,
            data.SelectedClientId,
            data.SelectedStatusId,
            data.SelectedMemberIds,
            imageName
        );

        if (result.Succeeded)
        {
            return Ok(new { success = true });
        }

        return BadRequest(new { success = false, error = result.Error });
    }
}
