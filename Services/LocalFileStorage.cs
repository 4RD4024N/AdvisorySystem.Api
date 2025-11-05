using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace AdvisorySystem.Api.Services
{
    public class LocalFileStorage : IFileStorage
    {
        private readonly IWebHostEnvironment _env;
        private readonly IConfiguration _cfg;
        public LocalFileStorage(IWebHostEnvironment env, IConfiguration cfg) { _env = env; _cfg = cfg; }

        public async Task<(string path, long size)> SaveAsync(IFormFile file, string subFolder, CancellationToken ct = default)
        {
            var root = _cfg["Storage:Root"] ?? "wwwroot/uploads";
            var folder = Path.Combine(_env.ContentRootPath, root, subFolder);
            Directory.CreateDirectory(folder);

            var safeName = Path.GetFileName(file.FileName);
            var name = $"{Guid.NewGuid():N}-{safeName}";
            var fullPath = Path.Combine(folder, name);

            await using var s = File.Create(fullPath);
            await file.CopyToAsync(s, ct);
            return (Path.GetRelativePath(_env.ContentRootPath, fullPath).Replace("\\", "/"), s.Length);
        }

        public FileStream Open(string path)
        {
            var full = Path.Combine(_env.ContentRootPath, path);
            return File.OpenRead(full);
        }
    }
}
