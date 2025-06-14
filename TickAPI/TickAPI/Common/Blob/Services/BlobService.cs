using Azure.Identity;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
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
        var contentType = image.ContentType;
        var stream = new MemoryStream();
        await image.CopyToAsync(stream);
        stream.Position = 0;
        
        var uploadOptions = new BlobUploadOptions
        {
            HttpHeaders = new BlobHttpHeaders
            {
                ContentType = contentType
            }
        };
        
        var response = await blob.UploadAsync(stream, uploadOptions);
        
        return blob.Uri.ToString();
    }
}