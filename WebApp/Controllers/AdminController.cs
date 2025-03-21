using Business.Interfaces;
using Data.Entities;
using Microsoft.AspNetCore.Mvc;

[Route("admin")]
public class AdminController : Controller
{
    private readonly IMemberService _memberService;

    public AdminController(IMemberService memberService)
    {
        _memberService = memberService;
    }

    [Route("members")]
    //public async Task<IActionResult> Members()
    //{
    //var allMembers = await _memberService.GetAllMembersAsync();
    //return View(allMembers); // Returnerar IEnumerable<MemberEntity>
    // }

    public IActionResult Members()
    {
        // Dummy-lista med testanvändare
        var testMembers = new List<MemberEntity>
    {
        new MemberEntity
        {
            Id = 1,
            FirstName = "Test",
            LastName = "User",
            Email = "test.user@example.com",
            Phone = "0701234567",
            JobTitle = "Developer",
            Address = "Testgatan 1",
            DateOfBirth = new DateTime(1990, 1, 1)
        },
        new MemberEntity
        {
            Id = 2,
            FirstName = "Anna",
            LastName = "Andersson",
            Email = "anna.andersson@example.com",
            Phone = "0702345678",
            JobTitle = "Designer",
            Address = "Exempelvägen 2",
            DateOfBirth = new DateTime(1985, 5, 15)
        },
        new MemberEntity
        {
            Id = 3,
            FirstName = "Erik",
            LastName = "Svensson",
            Email = "erik.svensson@example.com",
            Phone = "0703456789",
            JobTitle = "Manager",
            Address = "Demo Väg 3",
            DateOfBirth = new DateTime(1980, 3, 10)
        },
        new MemberEntity
        {
            Id = 4,
            FirstName = "Jeanette",
            LastName = "Mikkelsen",
            Email = "Jeanette.Mikkelsen@example.com",
            Phone = "070852852",
            JobTitle = "Confused",
            Address = "Demo Väg 3",
            DateOfBirth = new DateTime(1980, 3, 10)
        }
    };

        return View(testMembers);
    }

    [Route("clients")]
    public IActionResult Clients()
    {
        return View();
    }
}
