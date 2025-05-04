using Business.Interfaces;
using Business.Models;
using Domain.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using WebApp.Hubs;
using WebApp.ViewModels;
using Data.Entities;

namespace WebApp.Controllers;

/// <summary>
/// Handles Create, Read (edit), Update, and Delete operations for projects,
/// as well as adding members to existing projects.
/// Broadcasts real-time notifications to users via SignalR.
/// Accessible to users and administrators.
/// </summary>

[Authorize(Roles = "Admin,User")]
[Route("projectcrud")]
public class ProjectCrudController(
    IProjectService projectService,
    IClientService clientService,
    IMemberService memberService,
    IFileStorageService fileStorageService,
    INotificationService notificationService,
    IHubContext<NotificationHub> notificationHub,
    UserManager<ApplicationUser> userManager) : Controller
{
    private readonly IProjectService _projectService = projectService;
    private readonly IClientService _clientService = clientService;
    private readonly IMemberService _memberService = memberService;
    private readonly IFileStorageService _fileStorageService = fileStorageService;
    private readonly INotificationService _notificationService = notificationService;
    private readonly IHubContext<NotificationHub> _notificationHub = notificationHub;
    private readonly UserManager<ApplicationUser> _userManager = userManager;

    private const string ProjectUploadsFolder = "projects/projectuploads";
    private const string AvatarsFolder = "projects/avatars";

    /// <summary>
    /// POST /projectcrud
    /// Validates and creates a new project with selected client and members.
    /// Saves an uploaded image or assigns a random avatar.
    /// Creates and broadcasts a "New project" notification to all users in the "User" role.
    /// Returns a JSON object containing the redirect URL on success.
    /// </summary>
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

        // Determine the image name: upload or random avatar
        var data = vm.FormData;

        var imageName = data.Form.ProjectImage is { Length: > 0 }
            ? await _fileStorageService.SaveFileAsync(data.Form.ProjectImage, ProjectUploadsFolder)
            : _fileStorageService.GetRandomAvatar(AvatarsFolder);

        // Create the project via the service layer
        var result = await _projectService.CreateAsync(
            data.Form,
            data.SelectedClientId,
            data.SelectedMemberIds,
            imageName
        );

        if (!result.Succeeded)
            return BadRequest(new { success = false, error = result.Error ?? "Failed to create project." });

        // Persist a notification entity
        var notification = new NotificationEntity
        {
            Message = $"New project: {data.Form.ProjectName}",
            Image = imageName,
            NotificationTypeId = 2,
            NotificationTargetGroupId = 2,
            Created = DateTime.UtcNow
        };

        await _notificationService.AddNotificationAsync(notification);

        // Broadcast real-time notification to all users
        var users = await _userManager.GetUsersInRoleAsync("User");
        foreach (var user in users)
        {
            await _notificationHub.Clients.User(user.Id).SendAsync("RecieveNotification", new
            {
                id = notification.Id,
                message = notification.Message,
                imagePath = notification.Image,
                created = notification.Created
            });
        }

        // Return the URL to redirect the client to the Projects list
        return Ok(new { redirectUrl = Url.Action("Projects", "Projects") });
    }

    /// <summary>
    /// GET /projectcrud/edit/{id}
    /// Loads existing project data along with all clients and members,
    /// builds an EditProjectFormViewModel, and returns it as a partial view.
    /// </summary>
    [HttpGet("edit/{id}")]
    public async Task<IActionResult> EditProject(string id)
    {
        // Fetch the project from the service
        var projectResult = await _projectService.GetProjectByIdAsync(id);
        if (!projectResult.Succeeded || projectResult.Result == null)
            return NotFound();

        var project = projectResult.Result;

        // Load  data for clients and members
        var clientsResult = await _clientService.GetAllAsync();
        var membersResult = await _memberService.GetAllMembersAsync();

        // Ensure the project’s existing members appear first, then all others
        var allMembers = (membersResult.Result ?? Enumerable.Empty<MemberModel>())
            .UnionBy(project.MemberModels, m => m.Id)
            .ToList();

        // Construct the view model
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
                SelectedMemberIds = project.MemberModels.Select(m => m.Id).ToList()
            },
            Clients = clientsResult.Result ?? Enumerable.Empty<ClientModel>(),
            Members = allMembers
        };

        // Prepopulate the raw IDs field for the tag selector
        vm.FormData.SelectedMemberIdsRaw = string.Join(",", vm.FormData.SelectedMemberIds);

        return PartialView("~/Views/Shared/Partials/Sections/_EditProjectForm.cshtml", vm);
    }

    /// <summary>
    /// POST /projectcrud/edit
    /// Validates and updates an existing project.
    /// Saves a new uploaded image if provided.
    /// </summary>
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

        // Parse the comma-separated member IDs into a List<int>
        data.SelectedMemberIds = (data.SelectedMemberIdsRaw ?? "")
            .Split(",", StringSplitOptions.RemoveEmptyEntries)
            .Where(id => int.TryParse(id, out _))
            .Select(id => int.Parse(id))
            .ToList();

        // Save new project image if uploaded, else retain the old one
        var newImageName = data.Form.ProjectImage is { Length: > 0 }
            ? await _fileStorageService.SaveFileAsync(data.Form.ProjectImage, ProjectUploadsFolder)
            : data.Form.ImageName;

        // Perform the update
        var result = await _projectService.UpdateProjectAsync(
            data.Form,
            data.SelectedClientId,
            data.SelectedMemberIds,
            newImageName
        );

        return result.Succeeded
            ? Ok(new { success = true })
            : BadRequest(new { success = false, error = result.Error });
    }

    /// <summary>
    /// POST /projectcrud/delete/{id}
    /// Deletes the project with the given ID.
    /// </summary>
    [HttpPost("delete/{id}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteProject(string id)
    {
        var result = await _projectService.DeleteProjectAsync(id);
        return result.Succeeded
            ? Ok(new { success = true })
            : BadRequest(new { success = false, error = result.Error });
    }

    /// <summary>
    /// GET /projectcrud/add-member-modal/{id}
    /// Loads a modal partial for adding a member to the specified project.
    /// Returns a list of all members in the view model.
    /// </summary>
    [HttpGet("add-member-modal/{id}", Name = "AddMemberModal")]
    public async Task<IActionResult> LoadAddMemberToProjectModal(string id)
    {
        var membersResult = await _memberService.GetAllMembersAsync();
        if (!membersResult.Succeeded)
            return Problem(membersResult.Error ?? "Could not load members");

        var vm = new AddMemberToProjectViewModel
        {
            FormData = new AddMemberToProjectForm { ProjectId = id },
            Members = membersResult.Result ?? Array.Empty<MemberModel>()
        };

        return PartialView(
            "~/Views/Shared/Partials/Sections/_AddMemberToProjectForm.cshtml",
            vm
        );
    }

    /// <summary>
    /// POST /projectcrud/add-member
    /// Adds a selected member to an existing project.
    /// Validates input, updates the project’s member list via the service,
    /// and returns success or error in JSON.
    /// </summary>
    [HttpPost("add-member", Name = "AddMemberToProject")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddMemberToProject(AddMemberToProjectViewModel vm)
    {
        if (vm.FormData == null
            || string.IsNullOrEmpty(vm.FormData.ProjectId)
            || !vm.FormData.SelectedMemberId.HasValue)
        {
            return BadRequest(new { success = false, error = "Invalid form data." });
        }

        var projectResult = await _projectService.GetProjectByIdAsync(vm.FormData.ProjectId);
        if (!projectResult.Succeeded || projectResult.Result == null)
        {
            return BadRequest(new { success = false, error = "Project not found." });
        }

        var project = projectResult.Result;

        var memberId = vm.FormData.SelectedMemberId.Value;
        if (!project.MemberModels.Any(m => m.Id == memberId))
        {
            project.MemberModels.Add(new MemberModel { Id = memberId });

            var updateResult = await _projectService.UpdateProjectAsync(
                form: new EditProjectForm
                {
                    Id = project.Id,
                    ProjectName = project.ProjectName,
                    Description = project.Description ?? string.Empty,
                    StartDate = project.StartDate,
                    EndDate = project.EndDate,
                    Budget = project.Budget,
                    ImageName = project.ImageName
                },
                clientId: project.ClientModel.ClientId,
                memberIds: project.MemberModels.Select(m => m.Id).ToList(),
                newImageName: project.ImageName
            );

            if (!updateResult.Succeeded)
            {
                return BadRequest(new
                {
                    success = false,
                    error = updateResult.Error ?? "Failed to add member."
                });
            }
        }

        return Ok(new { success = true });
    }
}
