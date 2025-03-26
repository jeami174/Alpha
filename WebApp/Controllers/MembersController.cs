using Business.Interfaces;
using Business.Models;
using Microsoft.AspNetCore.Mvc;
using WebApp.Helpers;

namespace WebApp.Controllers
{
    public class MembersController : Controller
    {
        private readonly IMemberService _memberService;
        private readonly IFileStorageService _fileStorageService;

        public MembersController(IMemberService memberService, IFileStorageService fileStorageService)
        {
            _memberService = memberService;
            _fileStorageService = fileStorageService;
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
            string imageName = form.MemberImage != null && form.MemberImage.Length > 0
            ? await _fileStorageService.SaveFileAsync(form.MemberImage, "useruploads")
            : _fileStorageService.GetRandomAvatar();

                await _memberService.AddMemberAsync(form, imageName);
                return Ok(new { success = true });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, error = ex.Message });
            }
        }
    }
}
