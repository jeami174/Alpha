using Business.Interfaces;
using Business.Models;
using Domain.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;


namespace WebApp.Controllers;

[Authorize(Policy = "Users")]
[Route("admin")]
public class AdminController(IMemberService memberService, IClientService clientService) : Controller
{
private readonly IMemberService _memberService = memberService;
private readonly IClientService _clientService = clientService;

[Route("")]
public IActionResult Index()
{
    return View();
}

/// <summary>
/// GET /admin/members
/// Retrieves all members, applies optional search filtering, and renders the members view.
/// Only accessible to users in the “Admins” policy.
/// </summary>
[Authorize(Policy = "Admins")]
[HttpGet("members")]
public async Task<IActionResult> Members(string? query = null)
{
        // Call service to fetch all members
        var result = await _memberService.GetAllMembersAsync();
    if (!result.Succeeded)
            // If service failed, return a generic problem response
            return Problem(result.Error ?? "Could not load members");

        // Convert to a mutable list for filtering
        var members = result.Result!.ToList();

        // If a search query was provided, filter by first name, last name or email
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
        // Render the view with the (filtered) member list
        return View(members);
}

/// <summary>
/// GET /admin/clients
/// Retrieves all clients, applies optional search filtering, prepares a view model
/// including a blank AddClientForm, and renders the clients view.
/// Only accessible to users in the “Admins” policy.
/// </summary>
[Authorize(Policy = "Admins")]
[HttpGet("clients")]
public async Task<IActionResult> Clients(string? query = null)
{
    // Fetch all clients from the service
    var result = await _clientService.GetAllAsync();
    if (!result.Succeeded)
        return Problem(result.Error ?? "Could not load clients");
       
    // Convert result to a list (fallback to empty list if null)
    var clients = result.Result?.ToList() ?? new List<ClientModel>();

    // Apply optional search filtering on client name or email
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

        // Build a page view model with the filtered clients and an empty form for adding new clients
        var viewModel = new Domain.ViewModels.ClientPageViewModel
        {
            Clients = clients,
            NewClientForm = new AddClientForm()
        };

        // Preserve the search query in ViewBag so the UI can show it
        ViewBag.Query = query;
       
        // Render the clients page
        return View(viewModel);
}
}

