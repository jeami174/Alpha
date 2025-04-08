using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using WebApp.ViewModels;
using Data.Entities;
using Data.Interfaces;

namespace WebApp.ViewComponents;

public class TopbarViewComponent : ViewComponent
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IMemberRepository _memberRepository;

    public TopbarViewComponent(UserManager<ApplicationUser> userManager, IMemberRepository memberRepository)
    {
        _userManager = userManager;
        _memberRepository = memberRepository;
    }

    public async Task<IViewComponentResult> InvokeAsync()
    {
        var user = await _userManager.GetUserAsync(UserClaimsPrincipal);

        if (user == null)
        {
            return View("~/Views/Shared/Partials/Sections/_Topbar.cshtml", new UserProfileViewModel
            {
                FullName = "Unknown",
                ImagePath = "/uploads/members/avatars/default.svg"
            });
        }

        var member = await _memberRepository.GetOneAsync(m => m.UserId == user.Id);

        var rawImage = member?.ImageName;
        var path = !string.IsNullOrEmpty(rawImage)
            ? (rawImage.Contains("avatars/") || rawImage.Contains("useruploads/")
                ? "/" + rawImage.Replace("\\", "/")
                : "/uploads/useruploads/" + rawImage)
            : "/uploads/members/avatars/default.svg";

        var viewModel = new UserProfileViewModel
        {
            FullName = $"{user.FirstName} {user.LastName}",
            ImagePath = path
        };

        return View("~/Views/Shared/Partials/Sections/_Topbar.cshtml", viewModel);
    }

}
