using Business.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace WebApp.Controllers;

/// <summary>
/// Provides an endpoint for tag-based member search, 
/// used by autocomplete or tag selector components.
/// </summary>
public class TagsController(IMemberService memberService) : Controller
{
    private readonly IMemberService _memberService = memberService;

    /// <summary>
    /// GET /Tags/SearchTags?term={term}
    /// - If the search term is null or whitespace, returns an empty JSON array.
    /// - Otherwise, calls the member service to find matching members.
    /// - Projects each member to an object with:
    ///     • id: the member’s identifier
    ///     • tagName: the concatenated first and last name
    ///     • imageUrl: the member’s avatar or image path
    /// - Returns JSON of the resulting list, or HTTP&nbsp;500 on error.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> SearchTags(string term)
    {
        try
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
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            return StatusCode(500, new { error = "An error occurred while searching for tags." });
        }
    }

}