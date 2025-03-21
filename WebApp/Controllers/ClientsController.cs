using Business.Models;
using Microsoft.AspNetCore.Mvc;

namespace WebApp.Controllers;

public class ClientsController : Controller
{
    // private readonly IClientService _clientService;

    [HttpPost]
    public IActionResult AddClient(AddClientForm form)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState
                .Where(x => x.Value?.Errors.Count > 0)
                .ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value?.Errors.Select(x => x.ErrorMessage).ToArray()
                );

            return BadRequest(new { success = false, errors });
        }

        //var result = await _clientService.AddClientAsync(form);
        //if (result)
        //{
        //    return Ok(new { success = true });
        //}
        //else
        //{
        // return Problem("unable to submit data")
        //}

        return Ok(new { success = true }); //radera senare
    }


    [HttpPost]
    public IActionResult EditClient(EditClientForm form)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState
                .Where(x => x.Value?.Errors.Count > 0)
                .ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value?.Errors.Select(x => x.ErrorMessage).ToArray()
                );

            return BadRequest(new { success = false, errors });
        }

        //var result = await _clientService.UpdateClientAsync(form);
        //if (result)
        //{
        //    return Ok(new { success = true });
        //}
        //else
        //{
        // return Problem("unable to submit data")
        //}
        return Ok(new { success = true }); // radera senare
    }

}
