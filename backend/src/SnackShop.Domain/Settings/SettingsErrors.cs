using SnackShop.Domain.Common;

namespace SnackShop.Domain.Settings;

public static class SettingsErrors
{
    public static readonly Error NotSeeded = Error.Failure(
        "Settings.NotSeeded",
        "Shop settings have not been initialised. Run the database seeder.");

    public static readonly Error FreeShippingThresholdMustBePositive = Error.Validation(
        "Settings.FreeShippingThresholdMustBePositive",
        "Free shipping threshold must be greater than zero, or omitted to disable free shipping.");

    public static readonly Error FreeShippingThresholdTooLow = Error.Validation(
        "Settings.FreeShippingThresholdTooLow",
        "Free shipping threshold must be greater than the flat shipping fee.");

    public static readonly Error UnpaidExpiryOutOfRange = Error.Validation(
        "Settings.UnpaidExpiryOutOfRange",
        $"Unpaid bank transfer expiry must be between {ShopSettings.MinUnpaidExpiryHours} and {ShopSettings.MaxUnpaidExpiryHours} hours.");
}
