using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace AdvisorySystem.Api.Services
{
 public interface IFileStorage
 {
 Task<(string path, long size)> SaveAsync(IFormFile file, string subFolder, CancellationToken ct = default);
 FileStream Open(string path);
 Task<Stream> GetAsync(string path);
 Task DeleteAsync(string path);
 Task<bool> ExistsAsync(string path);
 Task<IEnumerable<string>> ListAsync(string prefix);
 }
}
