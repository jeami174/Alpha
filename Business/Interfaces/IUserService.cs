using Business.Models;
using Data.Entities;

namespace Business.Interfaces;
public interface IUserService
{
    Task<ServiceResult<ApplicationUser>> CreateUserAsync(string email, string password, string firstName, string lastName, string role);
    Task<ServiceResult<bool>> AddUserToRoleAsync(string userId, string roleName);
}