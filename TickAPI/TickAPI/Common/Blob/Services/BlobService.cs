using Azure.Identity;
using Azure.Storage.Blobs;
using TickAPI.Common.Blob.Abstractions;

namespace TickAPI.Common.Blob.Services;

public class BlobService : IBlobService
{
    private string _connectionString;
    private string _containerName;
    
    public BlobService(IConfiguration configuration)
    {
        _connectionString = configuration["BlobStorage:ConnectionString"];
        _containerName = configuration["BlobStorage:ContainerName"];
    }

    public async Task<string> UploadToBlobContainerAsync(string name, IFormFile image)
    {
        var container =  new BlobContainerClient(_connectionString, _containerName);
        Guid id = Guid.NewGuid();
        string blobName = name + id;
        var blob =  container.GetBlobClient(blobName);
        var stream = new MemoryStream();
        await image.CopyToAsync(stream);
        var response = blob.Upload(stream);
        return blob.Uri.ToString();
    }
}