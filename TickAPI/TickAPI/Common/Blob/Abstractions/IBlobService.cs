namespace TickAPI.Common.Blob.Abstractions;

public interface IBlobService
{
    public Task<string> UploadToBlobContainerAsync(string name, IFormFile image);
}