using Microsoft.AspNetCore.Mvc;

namespace WebApp.Controllers;

[Route("Admin")]
public class AdminController : Controller
{
    public IActionResult Members()
    {
        return View();
    }

    [Route("Clients")]
    public IActionResult Clients()
    {
        return View();
    }
}
