using Business.Interfaces;
using Business.Models;
using Data.Entities;
using Microsoft.AspNetCore.Identity;

namespace Business.Services;

/// <summary>
/// Handles user creation and role assignment using ASP.NET Identity,
/// including creating new users with profile details and adding them to roles.
/// </summary>
public class UserService(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager) : IUserService
{
    private readonly UserManager<ApplicationUser> _userManager = userManager;
    private readonly RoleManager<IdentityRole> _roleManager = roleManager;

    public async Task<ServiceResult<ApplicationUser>> CreateUserAsync(string email, string password, string firstName, string lastName, string role)
    {
        var user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            FirstName = firstName,
            LastName = lastName
        };

        var createResult = await _userManager.CreateAsync(user, password);

        if (!createResult.Succeeded)
            return ServiceResult<ApplicationUser>.Failure(string.Join(", ", createResult.Errors.Select(e => e.Description)));

        var roleResult = await AddUserToRoleAsync(user.Id, role);
        if (!roleResult.Succeeded)
            return ServiceResult<ApplicationUser>.Failure(roleResult.Error!);

        return ServiceResult<ApplicationUser>.Success(user);
    }

    public async Task<ServiceResult<bool>> AddUserToRoleAsync(string userId, string roleName)
    {
        if (!await _roleManager.RoleExistsAsync(roleName))
            return ServiceResult<bool>.Failure("Role does not exist.");

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return ServiceResult<bool>.Failure("User does not exist.");

        var result = await _userManager.AddToRoleAsync(user, roleName);
        return result.Succeeded
            ? ServiceResult<bool>.Success(true)
            : ServiceResult<bool>.Failure("Failed to assign role.");
    }
}
