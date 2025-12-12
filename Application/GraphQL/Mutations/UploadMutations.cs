using HotChocolate;
using HotChocolate.Authorization;
using HotChocolate.Types;
using MyShopServer.Application.Services.Interfaces;
using MyShopServer.Domain.Enums;
using MyShopServer.DTOs.Uploads;

namespace MyShopServer.Application.GraphQL.Mutations;

[ExtendObjectType(typeof(Mutation))]
public class UploadMutations
{
    private const long MaxBytes = 10 * 1024 * 1024; // 10MB

    [Authorize(Roles = new[] { nameof(RoleName.Admin), nameof(RoleName.Moderator) })]
    public async Task<UploadImageResultDto> UploadProductAsset(
        IFile file,
        [Service] IImageStorageService storage,
        [Service] IConfiguration config,
        CancellationToken ct)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(file.ContentType) || !file.ContentType.StartsWith("image/"))
                throw new Exception("Only image files are allowed.");

            if (file.Length is not null && file.Length > MaxBytes)
                throw new Exception($"File too large. Max {(MaxBytes / (1024 * 1024))}MB");

            await using var stream = file.OpenReadStream();

            var root = config.GetSection("Cloudinary")["Folder"] ?? "myshop";
            var folder = $"{root}/temp-products"; // folder chung cho ảnh đang tạo product

            var uploaded = await storage.UploadImageAsync(stream, file.Name, folder, ct);

            return new UploadImageResultDto
            {
                StatusCode = 201,
                Success = true,
                Message = "Upload success",
                Data = new UploadedImageDto
                {
                    Url = uploaded.Url,
                    PublicId = uploaded.PublicId
                }
            };
        }
        catch (Exception ex)
        {
            return new UploadImageResultDto
            {
                StatusCode = 400,
                Success = false,
                Message = ex.Message,
                Data = null
            };
        }
    }

    [Authorize(Roles = new[] { nameof(RoleName.Admin), nameof(RoleName.Moderator) })]
    public async Task<BoolResultDto> DeleteUploadedAsset(
        string publicId,
        [Service] IImageStorageService storage,
        CancellationToken ct)
    {
        try
        {
            await storage.DeleteByPublicIdAsync(publicId, ct);

            return new BoolResultDto
            {
                StatusCode = 200,
                Success = true,
                Message = "Delete success",
                Data = true
            };
        }
        catch (Exception ex)
        {
            return new BoolResultDto
            {
                StatusCode = 400,
                Success = false,
                Message = ex.Message,
                Data = false
            };
        }
    }
}
