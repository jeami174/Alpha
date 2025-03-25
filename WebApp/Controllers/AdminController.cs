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
                // Hämta alla medlemmar från business-lagret
                IEnumerable<MemberEntity> allMembers = await _memberService.GetAllMembersAsync();
                return View(allMembers);
            }
            catch (Exception ex)
            {
                // Hantera fel – här kan du antingen visa en felvy eller skicka en tom lista
                // Du kan logga felet och/eller returnera en error view
                // return View("Error", ex);
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

