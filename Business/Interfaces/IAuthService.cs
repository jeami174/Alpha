using Business.Models;
public interface IAuthService
{
    Task<ServiceResult<string>> RegisterAsync(SignUpFormModel form);
    Task<ServiceResult<string>> SignInAsync(SignInFormModel form);
    Task<ServiceResult<bool>> LogOutAsync();
    Task<ServiceResult<string>> GeneratePasswordResetTokenAsync(string email);
    Task<ServiceResult<bool>> ResetPasswordAsync(ResetPasswordFormModel form);
}
