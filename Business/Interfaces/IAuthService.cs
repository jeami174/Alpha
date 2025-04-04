using Business.Models;

namespace Business.Interfaces;

public interface IAuthService
{
    Task<ServiceResult<bool>> RegisterAsync(SignUpFormModel form);
    Task<ServiceResult<bool>> SignInAsync(SignInFormModel form);
    Task<ServiceResult<bool>> LogOutAsync();
}
