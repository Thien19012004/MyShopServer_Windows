namespace MyShopServer.Application.Common;

public static class PriceCalc
{
    public static int ApplyDiscount(int salePrice, int discountPct)
    {
        discountPct = Math.Clamp(discountPct, 0, 100);
        // integer price, tính bằng decimal để chuẩn
        return (int)Math.Round(salePrice * (100 - discountPct) / 100m, MidpointRounding.AwayFromZero);
    }
}
