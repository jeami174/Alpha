using Business.Interfaces;
using Business.Models;
using Microsoft.AspNetCore.Mvc;

namespace WebApp.Controllers;

public class MembersController : Controller
{
    private readonly IMemberService _memberService;

    public MembersController(IMemberService memberService)
    {
        _memberService = memberService;
    }

    // Visar en enskild medlems detaljer

    [HttpGet]
    public async Task<IActionResult> Member(int id)
    {
        var member = await _memberService.GetMemberByIdAsync(id);
        if (member == null)
            return NotFound();

        return View(member);
    }

    // Visar redigeringsformuläret med existerande medlemsdata

    [HttpGet]
    public async Task<IActionResult> EditMember(int id)
    {
        var member = await _memberService.GetMemberByIdAsync(id);
        if (member == null)
            return NotFound();

        // Mappar från MemberEntity till EditMemberForm
        var editForm = new EditMemberForm
        {
            FirstName = member.FirstName,
            LastName = member.LastName,
            MemberEmail = member.Email,
            Address = member.Address,
            Phone = member.Phone,
            JobTitle = member.JobTitle,
            DateOfBirth = member.DateOfBirth
        };

        return PartialView("_EditMemberForm", editForm);
    }

    // Hanterar uppdatering av en medlem med EditMemberForm

    [HttpPost]
    public async Task<IActionResult> EditMember(int id, EditMemberForm form)
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

        try
        {
            await _memberService.UpdateMemberAsync(id, form);
            return Ok(new { success = true });
        }
        catch (Exception ex)
        {
            return BadRequest(new { success = false, error = ex.Message });
        }
    }

    // Hanterar skapandet av en ny medlem (använder AddMemberForm)

    [HttpPost]
    public async Task<IActionResult> AddMember(AddMemberForm form)
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

        try
        {
            await _memberService.AddMemberAsync(form);
            return Ok(new { success = true });
        }
        catch (Exception ex)
        {
            return BadRequest(new { success = false, error = ex.Message });
        }
    }
}
