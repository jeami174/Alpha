using Business.Interfaces;
using Business.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApp.Controllers;

[Authorize]
[Route("admin")]
public class AdminController : Controller
{
    private readonly IMemberService _memberService;

    public AdminController(IMemberService memberService)
    {
        _memberService = memberService;
    }

    [Route("")]
    public IActionResult Index()
    {
        return View();
    }


    [Route("members")]
    public async Task<IActionResult> Members()
    {
        try
        {
            IEnumerable<MemberModel> allMembers = await _memberService.GetAllMembersAsync();
            return View(allMembers);
        }
        catch (Exception ex)
        {
            return View(new List<MemberModel>());
        }
    }


    [Route("clients")]
    public IActionResult Clients()
    {
        return View();
    }
}
