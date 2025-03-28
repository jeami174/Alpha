using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApp.Controllers;



[Authorize]

[Route("Projects")]
public class ProjectsController : Controller
{
    [Route("")]
    public IActionResult Projects()
    {
        return View();
    }
}

