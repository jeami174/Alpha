using Microsoft.AspNetCore.Mvc;

namespace WebApp.Controllers;

[Route("Projects")]
public class ProjectsController : Controller
{
    [Route("")]
    public IActionResult Projects()
    {
        return View();
    }
}

