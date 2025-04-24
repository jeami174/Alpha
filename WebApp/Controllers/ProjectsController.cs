using Business.Interfaces;
using Business.Models;
using Domain.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApp.ViewModels;

namespace WebApp.Controllers;

[Authorize(Policy = "Users")]
[Route("projects")]
public class ProjectsController : Controller
{
    private readonly IProjectService _projectService;
    private readonly IClientService _clientService;
    private readonly IStatusService _statusService;
    private readonly IMemberService _memberService;

    public ProjectsController(
        IProjectService projectService,
        IClientService clientService,
        IStatusService statusService,
        IMemberService memberService)
    {
        _projectService = projectService;
        _clientService = clientService;
        _statusService = statusService;
        _memberService = memberService;
    }

    [HttpGet("")]
    public async Task<IActionResult> Projects(string? status = null, string? sortBy = null)
    {
        var projectsResult = await _projectService.GetAllProjectsAsync(sortBy, status);
        var clientsResult = await _clientService.GetAllAsync();
        var statusesResult = await _statusService.GetAllAsync();
        var membersResult = await _memberService.GetAllMembersAsync();

        if (!projectsResult.Succeeded)
            return Problem(projectsResult.Error ?? "Could not load projects");

        var cards = projectsResult.Result!.Select(p => new ProjectCardViewModel
        {
            Id = p.Id,
            ProjectName = p.ProjectName,
            Description = p.Description,
            ClientName = p.ClientModel.ClientName,
            ImageName = p.ImageName ?? "uploads/projects/avatars/default.svg",
            StartDate = p.StartDate,
            EndDate = p.EndDate,
            MemberImageNames = p.MemberModels.Select(m => m.ImageName ?? "uploads/members/default.svg").ToList()
        }).ToList();

        var viewModel = new ProjectOverviewViewModel
        {
            Projects = cards,
            SelectedStatus = status,
            SortBy = sortBy,
            Clients = clientsResult.Result?.ToList() ?? [],
            Statuses = statusesResult.Result?.ToList() ?? [],
            Members = membersResult.Result?.ToList() ?? [],
            AddProjectForm = new AddProjectForm()
        };

        return View("Projects", viewModel);
    }
}