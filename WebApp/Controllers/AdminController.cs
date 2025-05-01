using Business.Interfaces;
using Business.Models;
using Domain.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;


namespace WebApp.Controllers;

[Authorize(Policy = "Users")]
[Route("admin")]
public class AdminController : Controller
{
private readonly IMemberService _memberService;
private readonly IClientService _clientService;

public AdminController(IMemberService memberService, IClientService clientService)
{
    _memberService = memberService;
    _clientService = clientService;
}

[Route("")]
public IActionResult Index()
{
    return View();
}

[Authorize(Policy = "Admins")]
[HttpGet("members")]
public async Task<IActionResult> Members(string? query = null)
{
    var result = await _memberService.GetAllMembersAsync();
    if (!result.Succeeded)
        return Problem(result.Error ?? "Could not load members");

    var members = result.Result!.ToList();

    if (!string.IsNullOrWhiteSpace(query))
    {
        members = members
            .Where(m =>
                m.FirstName.Contains(query, StringComparison.OrdinalIgnoreCase)
                || m.LastName.Contains(query, StringComparison.OrdinalIgnoreCase)
                || (m.FirstName + " " + m.LastName)
                    .Contains(query, StringComparison.OrdinalIgnoreCase)
                || (!string.IsNullOrWhiteSpace(m.Email)
                    && m.Email.Contains(query, StringComparison.OrdinalIgnoreCase))
            )
            .ToList();
    }
    return View(members);
}

[Authorize(Policy = "Admins")]
[HttpGet("clients")]
public async Task<IActionResult> Clients(string? query = null)
{
    var result = await _clientService.GetAllAsync();
    if (!result.Succeeded)
        return Problem(result.Error ?? "Could not load clients");

    var clients = result.Result?.ToList() ?? new List<ClientModel>();

    if (!string.IsNullOrWhiteSpace(query))
    {
        clients = clients
            .Where(c =>
                c.ClientName.Contains(query, StringComparison.OrdinalIgnoreCase)
                || (!string.IsNullOrWhiteSpace(c.ClientEmail)
                    && c.ClientEmail.Contains(query, StringComparison.OrdinalIgnoreCase))
            )
            .ToList();
    }

    var viewModel = new Domain.ViewModels.ClientPageViewModel
    {
        Clients = clients,
        NewClientForm = new AddClientForm()
    };

    ViewBag.Query = query;

    return View(viewModel);
}
}

