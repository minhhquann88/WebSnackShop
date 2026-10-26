namespace SnackShop.Domain.Common;

/// <summary>Business calendar helpers. Vietnam is UTC+7 with no daylight saving, so a fixed offset is exact.</summary>
public static class ShopTime
{
    public static readonly TimeSpan VietnamOffset = TimeSpan.FromHours(7);

    public static DateTimeOffset ToVietnam(DateTimeOffset instant) => instant.ToOffset(VietnamOffset);

    /// <summary>UTC instant of 00:00 Vietnam time on the calendar day containing <paramref name="instant"/>.</summary>
    public static DateTimeOffset StartOfVietnamDay(DateTimeOffset instant)
    {
        var local = ToVietnam(instant);
        return new DateTimeOffset(local.Year, local.Month, local.Day, 0, 0, 0, VietnamOffset).ToUniversalTime();
    }
}
