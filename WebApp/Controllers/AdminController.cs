using Business.Dtos;
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

    [HttpPost]
    public IActionResult AddClient(AddClientForm form)
    {
        if (!ModelState.IsValid)
        {
            return RedirectToAction("Clients");
        }
        return View();
    }

    [HttpPost]
    public IActionResult EditClient(AddClientForm form)
    {
        if (!ModelState.IsValid)
        {
            return RedirectToAction("Clients");
        }
        return View();
    }
}
