namespace MyShopServer.Application.Common;

/// <summary>
/// Helper for money/revenue calculations to avoid int overflow.
/// Project still stores money as int (VND), so this helper clamps results safely.
/// </summary>
public static class MoneyCalc
{
    public static int SafeAdd(int a, int b)
    {
        long sum = (long)a + b;
        return ClampToInt(sum);
    }

    public static int PercentOf(int amount, int percent)
    {
        percent = Math.Clamp(percent, 0, 100);
        if (amount <= 0 || percent <= 0) return 0;

        long value = (long)amount * percent / 100;
        return ClampToInt(value);
    }

    public static int ClampToInt(long value)
    {
        if (value <= 0) return 0;
        if (value >= int.MaxValue) return int.MaxValue;
        return (int)value;
    }
}
