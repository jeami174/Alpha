using System.Text.Json;
using Business.Models;
using Microsoft.AspNetCore.Mvc;

namespace WebApp.Controllers;

public class CookieController : Controller
{
    [HttpPost]
    public IActionResult SetCookies([FromBody] CookieConsent consent)
    {
    Response.Cookies.Append("SessionCookie", "Essential", new CookieOptions
    {
        IsEssential = true,
        Expires = DateTimeOffset.UtcNow.AddDays(90),
    });

    if (consent == null)
        return BadRequest();

    if (consent.Functional)
    {
        Response.Cookies.Append("FunctionalCookie", "Non-Essential", new CookieOptions
        {
            IsEssential = false,
            Expires = DateTimeOffset.UtcNow.AddDays(30),
            SameSite = SameSiteMode.Lax,
            Path = "/"
        });

    }
    else
    {
        Response.Cookies.Delete("FunctionalCookie");
    }

    if (consent.Marketing)
    {
        Response.Cookies.Append("MarketingCookie", "Non-Essential", new CookieOptions
        {
            IsEssential = false,
            Expires = DateTimeOffset.UtcNow.AddDays(30),
            SameSite = SameSiteMode.Lax,
            Path = "/"
        });
    }
    else
    {
        Response.Cookies.Delete("MarketingCookie");
    }

    Response.Cookies.Append("cookieConsent", JsonSerializer.Serialize(consent), new CookieOptions
    {
        IsEssential = true,
        Expires = DateTimeOffset.UtcNow.AddDays(90),
        SameSite = SameSiteMode.Lax,
        Path = "/"
    });

    return Ok();
    }
}
