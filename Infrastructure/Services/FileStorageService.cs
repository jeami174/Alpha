using Business.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

namespace Infrastructure.Services;

public class FileStorageService : IFileStorageService
{
    private readonly IWebHostEnvironment _env;

    public FileStorageService(IWebHostEnvironment env)
    {
        _env = env;
    }

    public async Task<string> SaveFileAsync(IFormFile file, string subFolder)
    {
        var extension = Path.GetExtension(file.FileName);
        var uniqueFileName = $"{Guid.NewGuid()}{extension}";

        var uploadFolder = Path.Combine(_env.WebRootPath, "uploads", "members", subFolder);
        Directory.CreateDirectory(uploadFolder);

        var filePath = Path.Combine(uploadFolder, uniqueFileName);
        using var stream = new FileStream(filePath, FileMode.Create);
        await file.CopyToAsync(stream);

        return Path.Combine("uploads", "members", subFolder, uniqueFileName).Replace("\\", "/");
    }

    public string GetRandomAvatar()
    {
        var avatarFolder = Path.Combine(_env.WebRootPath, "uploads", "members", "avatars");

        if (!Directory.Exists(avatarFolder))
            return "uploads/members/avatars/default.svg";

        var files = Directory.GetFiles(avatarFolder);
        if (files.Length == 0)
            return "uploads/members/avatars/default.svg";

        var random = new Random();
        var file = files[random.Next(files.Length)];
        return Path.Combine("uploads", "members", "avatars", Path.GetFileName(file)).Replace("\\", "/");
    }
}
