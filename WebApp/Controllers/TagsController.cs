using Business.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace WebApp.Controllers;
public class TagsController(IMemberService memberService) : Controller
{
    private readonly IMemberService _memberService = memberService;


    [HttpGet]
    public async Task<IActionResult> SearchTags(string term)
    {
        if (string.IsNullOrWhiteSpace(term))
            return Json(new List<object>());

        var members = await _memberService.SearchMembersAsync(term);

        var result = members.Select(m => new
        {
            id = m.Id,
            tagName = $"{m.FirstName} {m.LastName}",
            imageUrl = m.ImageName
        });

        return Json(result);
    }
}