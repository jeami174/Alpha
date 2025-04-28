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
    private readonly IMemberService _memberService;

    public ProjectsController(
        IProjectService projectService,
        IClientService clientService,
        IMemberService memberService)
    {
        _projectService = projectService;
        _clientService = clientService;
        _memberService = memberService;
    }

    [HttpGet("")]
    public async Task<IActionResult> Projects(string? status = null, string? sortBy = null)
    {
        var projectsResult = await _projectService.GetAllProjectsAsync();
        var clientsResult = await _clientService.GetAllAsync();
        var membersResult = await _memberService.GetAllMembersAsync();

        if (!projectsResult.Succeeded)
            return Problem(projectsResult.Error ?? "Could not load projects");

        var cards = projectsResult.Result!.Select(p => new ProjectCardViewModel
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
            MemberImageNames = p.MemberModels
                .Select(m => m.ImageName!)
                .ToList()
        }).ToList();

        var viewModel = new ProjectOverviewViewModel
        {
            Projects = cards,
            SelectedStatus = status,
            SortBy = sortBy,
            Clients = clientsResult.Result?.ToList() ?? [],
            Members = membersResult.Result?.ToList() ?? [],
            AddProjectForm = new AddProjectForm()
        };

        return View("Projects", viewModel);
    }
}