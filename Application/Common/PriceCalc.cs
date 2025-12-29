namespace MyShopServer.Application.Common;

public static class PriceCalc
{
    public static int ApplyDiscount(int salePrice, int discountPct)
    {
        discountPct = Math.Clamp(discountPct, 0, 100);

        var discounted = (decimal)salePrice * (100 - discountPct) / 100m;
        var finalPrice = (int)Math.Round(discounted, MidpointRounding.AwayFromZero);

        return Math.Max(0, finalPrice);
    }
}
