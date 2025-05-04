using Business.Interfaces;
using Business.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApp.ViewModels;

namespace WebApp.Controllers;

/// <summary>
/// Controller for displaying the project overview page.
/// both users and admins may access.
/// </summary>
[Authorize(Policy = "Users")]
[Route("projects")]
public class ProjectsController(
    IProjectService projectService,
    IClientService clientService,
    IMemberService memberService) : Controller
{
    private readonly IProjectService _projectService = projectService;
    private readonly IClientService _clientService = clientService;
    private readonly IMemberService _memberService = memberService;

    /// <summary>
    /// GET /projects
    /// Loads all projects (optionally sorted), maps them to card view models,
    /// applies an optional search filter, and populates additional dropdown data
    /// (clients and members) before rendering the overview view.
    /// </summary>
    [HttpGet("")]
    public async Task<IActionResult> Projects(
        string? status = null,
        string? sortBy = null,
        string? query = null)
    {
        var projectsResult = await _projectService.GetAllProjectsAsync(sortBy);
        if (!projectsResult.Succeeded)
            return Problem(projectsResult.Error ?? "Could not load projects");

        var cards = projectsResult.Result!
            .Select(p => new ProjectCardViewModel
            {
                Id = p.Id,
                ProjectName = p.ProjectName,
                Description = p.Description,
                ClientName = p.ClientModel.ClientName,
                ImageName = string.IsNullOrWhiteSpace(p.ImageName)
                                    ? "uploads/projects/avatars/default.svg"
                                    : p.ImageName.Replace("\\", "/"),
                StartDate = p.StartDate,
                EndDate = p.EndDate,
                MemberImageNames = p.MemberModels.Select(m => m.ImageName!).ToList()
            })
            .ToList();

        if (!string.IsNullOrWhiteSpace(query))
        {
            cards = cards
                .Where(c => c.ProjectName!
                    .Contains(query, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        var clientsResult = await _clientService.GetAllAsync();
        var membersResult = await _memberService.GetAllMembersAsync();

        var viewModel = new ProjectOverviewViewModel
        {
            Projects = cards,
            SortBy = sortBy,
            Clients = clientsResult.Result?.ToList() ?? new(),
            Members = membersResult.Result?.ToList() ?? new(),
            AddProjectForm = new AddProjectForm()
        };

        ViewBag.Query = query;

        return View("Projects", viewModel);
    }
}