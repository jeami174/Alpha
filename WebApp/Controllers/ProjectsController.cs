using Microsoft.AspNetCore.Mvc;

namespace WebApp.Controllers;

public class ProjectsController : Controller
{
    [Route("projects")] //Ändra här så att det inte blir projects/projects
    public IActionResult Projects()
    {
        return View();
    }
}
