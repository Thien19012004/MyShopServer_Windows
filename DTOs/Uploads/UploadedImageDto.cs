namespace MyShopServer.DTOs.Uploads;

public class UploadedImageDto
{
    public string Url { get; set; } = string.Empty;
    public string PublicId { get; set; } = string.Empty; // FE giữ để delete chắc chắn
}
