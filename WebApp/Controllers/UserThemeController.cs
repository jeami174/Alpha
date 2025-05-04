using Data.Context;
using Data.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

/// <summary>
/// API controller for retrieving and updating the current user's theme preference.
/// Exposes GET and POST endpoints at /api/user/theme.
/// </summary>
/// 
[ApiController]
[Route("api/user/theme")]
public class UserThemeController(UserManager<ApplicationUser> userManager, DataContext db) : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager = userManager;
    private readonly DataContext _db = db;

    [HttpGet]
    public async Task<IActionResult> GetTheme()
    {
        // Retrieve the currently logged-in user
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
            return Unauthorized();

        // Use stored preference or default to "light"
        var theme = user.PreferredTheme ?? "light";
        return Ok(new { theme });
    }

    [HttpPost]
    public async Task<IActionResult> SetTheme([FromBody] ThemeDto dto)
    {
        // Retrieve the currently logged-in user
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();
        
        // Update and save the preference
        user.PreferredTheme = dto.Theme;
        await _db.SaveChangesAsync();
        return NoContent();
    }
}

/// <summary>
/// Data Transfer Object for setting the user's theme preference.
/// </summary>
public class ThemeDto
{
    public string Theme { get; set; } = null!;
}
