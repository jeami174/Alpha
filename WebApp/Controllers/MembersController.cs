using Business.Interfaces;
using Business.Models;
using Microsoft.AspNetCore.Mvc;
using WebApp.Helpers;

namespace WebApp.Controllers;

public class MembersController : Controller
{
    private readonly IMemberService _memberService;
    private readonly IFileStorageService _fileStorageService;

    public MembersController(IMemberService memberService, IFileStorageService fileStorageService)
    {
        _memberService = memberService;
        _fileStorageService = fileStorageService;
    }

    public async Task<IActionResult> Index()
    {
        var members = await _memberService.GetAllMembersAsync();
        return View(members);
    }

    [HttpPost]
    public async Task<IActionResult> AddMember(AddMemberForm form)
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

        try
        {
            string imageName;

            if (form.MemberImage != null && form.MemberImage.Length > 0)
            {
                imageName = await _fileStorageService.SaveFileAsync(form.MemberImage, "useruploads");
            }
            else
            {
                imageName = _fileStorageService.GetRandomAvatar();
            }

            await _memberService.AddMemberAsync(form, imageName);
            return Ok(new { success = true });
        }
        catch (Exception ex)
        {
            return BadRequest(new { success = false, error = ex.Message });
        }
    }


    [HttpGet]
    public async Task<IActionResult> EditMember(int id)
    {
        var member = await _memberService.GetMemberByIdAsync(id);
        if (member == null)
        {
            return NotFound();
        }

        var editForm = new EditMemberForm
        {
            Id = member.Id,
            FirstName = member.FirstName,
            LastName = member.LastName,
            MemberEmail = member.Email,
            Phone = member.Phone,
            DateOfBirth = member.DateOfBirth,
            ImageName = member.ImageName,
            RoleName = member.Role?.Name,
            Street = member.Address?.Street,
            PostalCode = member.Address?.PostalCode,
            City = member.Address?.City
        };

        return PartialView("~/Views/Shared/Partials/Sections/_EditMemberForm.cshtml", editForm);
    }

    [HttpPost]
    public async Task<IActionResult> EditMember(EditMemberForm form)
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

        try
        {
            if (form.MemberImage != null && form.MemberImage.Length > 0)
            {
                string newImageName = await _fileStorageService.SaveFileAsync(form.MemberImage, "useruploads");
                form.ImageName = newImageName;
            }

            await _memberService.UpdateMemberAsync(form.Id, form);
            return Ok(new { success = true });
        }
        catch (Exception ex)
        {
            return BadRequest(new { success = false, error = ex.Message });
        }
    }
}

