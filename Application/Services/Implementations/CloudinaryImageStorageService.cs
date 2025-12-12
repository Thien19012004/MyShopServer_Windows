using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.Extensions.Options;
using MyShopServer.Application.Services.Interfaces;
using MyShopServer.Infrastructure.Cloudinary;

namespace MyShopServer.Application.Services.Implementations;

public class CloudinaryImageStorageService : IImageStorageService
{
    private readonly CloudinaryDotNet.Cloudinary _cloudinary;

    public CloudinaryImageStorageService(IOptions<CloudinaryOptions> options)
    {
        var opt = options.Value;
        var account = new Account(opt.CloudName, opt.ApiKey, opt.ApiSecret);
        _cloudinary = new CloudinaryDotNet.Cloudinary(account) { Api = { Secure = true } };
    }

    public async Task<StorageUploadResult> UploadImageAsync(
        Stream stream,
        string fileName,
        string folder,
        CancellationToken ct = default)
    {
        var uploadParams = new ImageUploadParams
        {
            File = new FileDescription(fileName, stream),
            Folder = folder,
            UseFilename = true,
            UniqueFilename = true,
            Overwrite = false
        };

        var result = await _cloudinary.UploadAsync(uploadParams, ct);

        if (result.Error != null)
            throw new Exception(result.Error.Message);

        var url = result.SecureUrl?.ToString();
        if (string.IsNullOrWhiteSpace(url) || string.IsNullOrWhiteSpace(result.PublicId))
            throw new Exception("Cloudinary upload failed: missing url/public_id.");

        return new StorageUploadResult(url, result.PublicId);
    }

    public async Task DeleteByPublicIdAsync(string publicId, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(publicId)) return;

        var delParams = new DeletionParams(publicId) { Invalidate = true };
        var res = await _cloudinary.DestroyAsync(delParams);

        if (res.Error != null)
            throw new Exception(res.Error.Message);
    }
}
