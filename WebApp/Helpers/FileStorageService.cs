
namespace WebApp.Helpers;

public interface IFileStorageService
{
    Task<string> SaveFileAsync(IFormFile file, string subFolder);
    string GetRandomAvatar();
}

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

        var relativePath = Path.Combine("uploads", "members", subFolder, uniqueFileName).Replace("\\", "/");
        return relativePath;
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
        var randomIndex = random.Next(files.Length);
        var fullPath = files[randomIndex];
        var fileName = Path.GetFileName(fullPath);

        var relativePath = Path.Combine("uploads", "members", "avatars", fileName).Replace("\\", "/");
        return relativePath;
    }
}
