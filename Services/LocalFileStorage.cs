using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.IO;

namespace AdvisorySystem.Api.Services
{
    public class LocalFileStorage : IFileStorage
    {
        private readonly string _uploadPath;
        private readonly ILogger<LocalFileStorage> _logger;

        public LocalFileStorage(IConfiguration config, ILogger<LocalFileStorage> logger)
        {
            _uploadPath = config["Storage:Root"] ?? "wwwroot/uploads";
            _logger = logger;

            if (!Directory.Exists(_uploadPath))
            {
                Directory.CreateDirectory(_uploadPath);
                _logger.LogInformation("Created upload directory: {Path}", _uploadPath);
            }
        }

        public async Task<(string path, long size)> SaveAsync(IFormFile file, string subFolder, CancellationToken ct = default)
        {
            var fileName = $"{subFolder}_{Guid.NewGuid()}_{file.FileName}";
            var fullPath = Path.Combine(_uploadPath, fileName);

            using var stream = new FileStream(fullPath, FileMode.Create);
            await file.CopyToAsync(stream, ct);

            _logger.LogInformation("File saved locally: {FileName}", fileName);
            return (fullPath, file.Length);
        }

        public FileStream Open(string path)
        {
            if (!File.Exists(path))
                throw new FileNotFoundException($"File not found: {path}");

            return new FileStream(path, FileMode.Open, FileAccess.Read);
        }

        public Task<Stream> GetAsync(string path)
        {
            if (!File.Exists(path))
                throw new FileNotFoundException($"File not found: {path}");

            return Task.FromResult<Stream>(new FileStream(path, FileMode.Open, FileAccess.Read));
        }

        public Task DeleteAsync(string path)
        {
            if (File.Exists(path))
            {
                File.Delete(path);
                _logger.LogInformation("File deleted: {Path}", path);
            }
            return Task.CompletedTask;
        }

        public Task<bool> ExistsAsync(string path)
        {
            return Task.FromResult(File.Exists(path));
        }

        public Task<IEnumerable<string>> ListAsync(string prefix)
        {
            var files = Directory.GetFiles(_uploadPath, $"{prefix}*");
            return Task.FromResult<IEnumerable<string>>(files);
        }
    }
}
