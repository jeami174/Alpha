using Business.Interfaces;
using Business.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApp.Controllers;

/// <summary>
/// Controller for managing clients. 
/// Provides endpoints to add, edit, and delete clients.
/// Requires the user to satisfy the “Admins” policy.
/// </summary>

[Authorize(Policy = "Admins")]
[Route("clients")]
public class ClientsController(IClientService clientService, IFileStorageService fileStorageService) : Controller
{
    private readonly IClientService _clientService = clientService;
    private readonly IFileStorageService _fileStorageService = fileStorageService;

    private const string ClientUploadsFolder = "clients/clientuploads";
    private const string ClientAvatarFolder = "Clients/avatars";

    [HttpPost("add")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddClient(AddClientForm form)
    { 
        // Ensure client form data passes model validation
        if (!ModelState.IsValid)
        {
            var errors = ModelState
                .Where(x => x.Value?.Errors.Count > 0)
                .ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value?.Errors.Select(e => e.ErrorMessage).ToArray()
                );

            return BadRequest(new { success = false, errors });
        }

        // Save uploaded file or pick a random avatar if none uploaded
        string imageName = form.ClientImage is { Length: > 0 }
            ? await _fileStorageService.SaveFileAsync(form.ClientImage, ClientUploadsFolder)
            : _fileStorageService.GetRandomAvatar(ClientAvatarFolder);
        
        // Delegate creation logic to the service layer
        var result = await _clientService.CreateAsync(form, imageName);

        if (result.Succeeded)
            return Ok(new { success = true });

        return BadRequest(new { success = false, error = result.Error ?? "Unable to add client" });
    }

    /// <summary>
    /// Handles GET /clients/edit/{id}.
    /// Loads the existing client data by ID and returns a partial view
    /// pre-populated with an EditClientForm model.
    /// Returns 404 Not Found if the client does not exist.
    /// </summary>
    [HttpGet("edit/{id}")]
    public async Task<IActionResult> EditClient(int id)
    {
        var result = await _clientService.GetByIdAsync(id);
        if (!result.Succeeded || result.Result == null)
            return NotFound();

        var client = result.Result;

        // Map the client data into the edit form DTO
        var form = new EditClientForm
        {
            Id = client.ClientId,
            ClientName = client.ClientName,
            ClientEmail = client.ClientEmail,
            Location = client.Location,
            Phone = client.Phone,
            ImageName = client.ImageName
        };

        // Return the form partial for AJAX rendering
        return PartialView("~/Views/Shared/Partials/Sections/_EditClientForm.cshtml", form);
    }

    /// <summary>
    /// Handles POST /clients/edit.
    /// Validates the edit form, saves a new image if one was uploaded,
    /// then calls the service to update the client.
    /// Returns 200 OK on success or 400 Bad Request with validation/errors.
    /// </summary>
    [HttpPost("edit")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditClient(EditClientForm form)
    {
        // Validate incoming form data
        if (!ModelState.IsValid)
        {
            var errors = ModelState
                .Where(x => x.Value?.Errors.Count > 0)
                .ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value?.Errors.Select(e => e.ErrorMessage).ToArray()
                );

            return BadRequest(new { success = false, errors });
        }
        
        // If a new image was uploaded, save it and update the form's ImageName
        if (form.ClientImage is { Length: > 0 })
        {
            string newImageName = await _fileStorageService.SaveFileAsync(form.ClientImage, ClientUploadsFolder);
            form.ImageName = newImageName;
        }

        var result = await _clientService.UpdateAsync(form);

        if (result.Succeeded)
            return Ok(new { success = true });

        return BadRequest(new { success = false, error = result.Error });
    }

    /// <summary>
    /// Handles POST /clients/delete.
    /// Deletes the client by ID and returns 200 OK on success
    /// or 400 Bad Request with an error message.
    /// </summary>
    [HttpPost("delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteClient(int id)
    {
        var result = await _clientService.DeleteAsync(id);
        return result.Succeeded
            ? Ok(new { success = true })
            : BadRequest(new { success = false, error = result.Error });
    }
}




