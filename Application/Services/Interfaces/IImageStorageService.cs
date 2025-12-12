namespace MyShopServer.Application.Services.Interfaces;

public record StorageUploadResult(string Url, string PublicId);

public interface IImageStorageService
{
    Task<StorageUploadResult> UploadImageAsync(
        Stream stream,
        string fileName,
        string folder,
        CancellationToken ct = default);

    Task DeleteByPublicIdAsync(string publicId, CancellationToken ct = default);
}
