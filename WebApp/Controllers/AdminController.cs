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
    [Route("members")]
    public async Task<IActionResult> Members()
    {
        var result = await _memberService.GetAllMembersAsync();
        return result.Succeeded
            ? View(result.Result)
            : Problem(result.Error ?? "Could not load members");
    }

    [Authorize(Policy = "Admins")]
    [Route("clients")]
    public async Task<IActionResult> Clients()
    {
        var result = await _clientService.GetAllAsync();

        if (!result.Succeeded)
            return Problem(result.Error ?? "Could not load clients");

        var viewModel = new Domain.ViewModels.ClientPageViewModel
        {
            Clients = result.Result ?? Enumerable.Empty<ClientModel>(),
            NewClientForm = new AddClientForm()
        };

        return View(viewModel);
    }
}

