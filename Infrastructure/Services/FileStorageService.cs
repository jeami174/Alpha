using Business.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

namespace Infrastructure.Services
{
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

            var uploadFolder = Path.Combine(_env.WebRootPath, "uploads", subFolder);
            Directory.CreateDirectory(uploadFolder);

            var filePath = Path.Combine(uploadFolder, uniqueFileName);
            using var stream = new FileStream(filePath, FileMode.Create);
            await file.CopyToAsync(stream);

            return Path.Combine("uploads", subFolder, uniqueFileName).Replace("\\", "/");
        }

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
}
