using Data.Context;
using Data.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/user/theme")]
public class UserThemeController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly DataContext _db;

    public UserThemeController(UserManager<ApplicationUser> userManager, DataContext db)
    {
        _userManager = userManager;
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> GetTheme()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
            return Unauthorized();

        var theme = user.PreferredTheme ?? "light";
        return Ok(new { theme });
    }

    [HttpPost]
    public async Task<IActionResult> SetTheme([FromBody] ThemeDto dto)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        user.PreferredTheme = dto.Theme;
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
public class ThemeDto
{
    public string Theme { get; set; } = null!;
}
