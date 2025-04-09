using Microsoft.AspNetCore.Http;

namespace Business.Interfaces;

public interface IFileStorageService
{
    Task<string> SaveFileAsync(IFormFile file, string subFolder);
    string GetRandomAvatar(string subFolder);
}
