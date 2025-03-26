using Business.Interfaces;
using Data.Entities;
using Microsoft.AspNetCore.Mvc;

namespace WebApp.Controllers
{
    [Route("admin")]
    public class AdminController : Controller
    {
        private readonly IMemberService _memberService;

        public AdminController(IMemberService memberService)
        {
            _memberService = memberService;
        }

        [Route("members")]
        public async Task<IActionResult> Members()
        {
            try
            {
                IEnumerable<MemberEntity> allMembers = await _memberService.GetAllMembersAsync();
                return View(allMembers);
            }
            catch (Exception ex)
            {

                return View(new List<MemberEntity>());
            }
        }

        [Route("clients")]
        public IActionResult Clients()
        {
            return View();
        }
    }
}

