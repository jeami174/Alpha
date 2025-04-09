using Business.Interfaces;
using Business.Models;
using Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApp.Controllers
{
    [Authorize(Policy = "Admins")]
    [Route("clients")]
    public class ClientsController : Controller
    {
        private readonly IClientService _clientService;
        private readonly IFileStorageService _fileStorageService;

        private const string ClientUploadsFolder = "clients/clientuploads";
        private const string ClientAvatarFolder = "Clients/avatars";

        public ClientsController(IClientService clientService, IFileStorageService fileStorageService)
        {
            _clientService = clientService;
            _fileStorageService = fileStorageService;
        }

        [HttpPost("add")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddClient(AddClientForm form)
        {
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

            string imageName = form.ClientImage is { Length: > 0 }
                ? await _fileStorageService.SaveFileAsync(form.ClientImage, ClientUploadsFolder)
                : _fileStorageService.GetRandomAvatar(ClientAvatarFolder);

            var result = await _clientService.CreateAsync(form, imageName);

            if (result.Succeeded)
                return Ok(new { success = true });

            return BadRequest(new { success = false, error = result.Error ?? "Unable to add client" });
        }

        [HttpGet("edit/{id}")]
        public async Task<IActionResult> EditClient(int id)
        {
            var result = await _clientService.GetByIdAsync(id);
            if (!result.Succeeded || result.Result == null)
                return NotFound();

            var client = result.Result;

            var form = new EditClientForm
            {
                Id = client.ClientId,
                ClientName = client.ClientName,
                ClientEmail = client.ClientEmail,
                Location = client.Location,
                Phone = client.Phone,
                ImageName = client.ImageName
            };

            return PartialView("~/Views/Shared/Partials/Sections/_EditClientForm.cshtml", form);
        }

        [HttpPost("edit")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditClient(EditClientForm form)
        {
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
}




