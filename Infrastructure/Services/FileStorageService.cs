using Business.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

namespace Infrastructure.Services;

/// <summary>
/// Handles file storage operations by saving uploaded files to the web root
/// and providing methods to retrieve or generate avatar image paths.
/// The code is my own, but I received guidance from ChatGPT to understand how to implement the functionality.
/// </summary>
public class FileStorageService(IWebHostEnvironment env) : IFileStorageService
{
    private readonly IWebHostEnvironment _env = env;

    /// <summary>
    /// Saves the provided IFormFile to disk under "wwwroot/uploads/{subFolder}",
    /// using a GUID-based filename for uniqueness, and returns the web-relative path.
    /// </summary>
    public async Task<string> SaveFileAsync(IFormFile file, string subFolder)
    {
        var extension = Path.GetExtension(file.FileName);
        var uniqueFileName = $"{Guid.NewGuid()}{extension}";

        var uploadFolder = Path.Combine(_env.WebRootPath, "uploads", subFolder);
        Directory.CreateDirectory(uploadFolder);

        var filePath = Path.Combine(uploadFolder, uniqueFileName);
        using var stream = new FileStream(filePath, FileMode.Create);
        await file.CopyToAsync(stream);

        return Path.Combine("uploads", subFolder, uniqueFileName).Replace("\\", "/");
    }

    /// <summary>
    /// Selects a random avatar image filename from "wwwroot/uploads/{subFolder}".
    /// Falls back to "default.svg" if the folder does not exist or is empty.
    /// </summary>
    public string GetRandomAvatar(string subFolder)
    {
        var avatarFolder = Path.Combine(_env.WebRootPath, "uploads", subFolder);

        if (!Directory.Exists(avatarFolder))
            return Path.Combine("uploads", subFolder, "default.svg").Replace("\\", "/");

        var files = Directory.GetFiles(avatarFolder);
        if (files.Length == 0)
            return Path.Combine("uploads", subFolder, "default.svg").Replace("\\", "/");

        var random = new Random();
        var file = files[random.Next(files.Length)];
        return Path.Combine("uploads", subFolder, Path.GetFileName(file)).Replace("\\", "/");
    }
}
