using System.Globalization;
using System.Text;

namespace MyShopServer.Application.Common;

public static class TextSearch
{
    public static string Normalize(string? input)
    {
        if (string.IsNullOrWhiteSpace(input)) return "";

        var formD = input.Trim().Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder(formD.Length);

        foreach (var ch in formD)
        {
            var uc = CharUnicodeInfo.GetUnicodeCategory(ch);
            if (uc != UnicodeCategory.NonSpacingMark)
                sb.Append(ch);
        }

        var s = sb.ToString()
            .Normalize(NormalizationForm.FormC)
            .Replace('đ', 'd').Replace('Đ', 'd')
            .ToLowerInvariant();

        // bỏ space/ký tự đặc biệt 
        var sb2 = new StringBuilder(s.Length);
        foreach (var ch in s)
            if (char.IsLetterOrDigit(ch)) sb2.Append(ch);

        return sb2.ToString();
    }
}
