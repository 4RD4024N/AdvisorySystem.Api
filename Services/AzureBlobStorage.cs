using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.AspNetCore.Http;

namespace AdvisorySystem.Api.Services;

public class AzureBlobStorage : IFileStorage
{
    private readonly BlobServiceClient _blobServiceClient;
    private readonly string _containerName;
    private readonly ILogger<AzureBlobStorage> _logger;

    public AzureBlobStorage(
     IConfiguration configuration,
        ILogger<AzureBlobStorage> logger)
    {
        var connectionString = configuration["Azure:StorageConnectionString"];
        _containerName = configuration["Azure:ContainerName"] ?? "documents";
        _logger = logger;

        if (string.IsNullOrEmpty(connectionString))
        {
   _logger.LogWarning("Azure Storage connection string not configured. Using local storage.");
            throw new InvalidOperationException("Azure Storage not configured");
        }

        _blobServiceClient = new BlobServiceClient(connectionString);
    }

    public async Task<(string path, long size)> SaveAsync(IFormFile file, string subFolder, CancellationToken ct = default)
    {
        try
        {
            // Get or create container
            var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
            await containerClient.CreateIfNotExistsAsync(PublicAccessType.None, cancellationToken: ct);

            // Generate unique filename
      var fileName = $"{subFolder}_{Guid.NewGuid()}_{file.FileName}";
            var blobClient = containerClient.GetBlobClient(fileName);

   // Set content type
    var blobHttpHeaders = new BlobHttpHeaders
{
                ContentType = file.ContentType
       };

            // Upload file
   using var stream = file.OpenReadStream();
        await blobClient.UploadAsync(stream, new BlobUploadOptions
    {
     HttpHeaders = blobHttpHeaders
 }, cancellationToken: ct);

       _logger.LogInformation("File uploaded to Azure Blob Storage: {FileName}", fileName);

   // Return blob URL and size
     return (blobClient.Uri.ToString(), file.Length);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to upload file to Azure Blob Storage");
     throw;
        }
    }

    public FileStream Open(string path)
    {
        throw new NotSupportedException("FileStream.Open is not supported for Azure Blob Storage. Use GetAsync instead.");
    }

    public async Task<Stream> GetAsync(string path)
    {
        try
     {
   // Extract blob name from URL
    var uri = new Uri(path);
 var blobName = uri.Segments[^1];

       var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
          var blobClient = containerClient.GetBlobClient(blobName);

     var response = await blobClient.DownloadAsync();
            return response.Value.Content;
        }
        catch (Exception ex)
      {
  _logger.LogError(ex, "Failed to download file from Azure Blob Storage: {Path}", path);
      throw;
    }
    }

    public async Task DeleteAsync(string path)
    {
     try
        {
 var uri = new Uri(path);
            var blobName = uri.Segments[^1];

   var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
     var blobClient = containerClient.GetBlobClient(blobName);

       await blobClient.DeleteIfExistsAsync();
  _logger.LogInformation("File deleted from Azure Blob Storage: {BlobName}", blobName);
      }
        catch (Exception ex)
        {
   _logger.LogError(ex, "Failed to delete file from Azure Blob Storage: {Path}", path);
            throw;
   }
  }

    public async Task<bool> ExistsAsync(string path)
 {
        try
        {
            var uri = new Uri(path);
       var blobName = uri.Segments[^1];

var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
     var blobClient = containerClient.GetBlobClient(blobName);

      return await blobClient.ExistsAsync();
        }
        catch
        {
            return false;
        }
 }

    public async Task<IEnumerable<string>> ListAsync(string prefix)
  {
    try
      {
        var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
     var blobs = new List<string>();

  await foreach (var blobItem in containerClient.GetBlobsAsync(prefix: prefix))
        {
       var blobClient = containerClient.GetBlobClient(blobItem.Name);
       blobs.Add(blobClient.Uri.ToString());
         }

 return blobs;
        }
catch (Exception ex)
     {
      _logger.LogError(ex, "Failed to list files from Azure Blob Storage");
            throw;
        }
    }
}
