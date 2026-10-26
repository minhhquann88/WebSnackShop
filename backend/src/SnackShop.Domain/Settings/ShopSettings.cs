using SnackShop.Domain.Common;

namespace SnackShop.Domain.Settings;

/// <summary>
/// Singleton row holding shop-wide configuration: shipping fee policy, bank account for transfers, contact info.
/// Always loaded via <see cref="IShopSettingsRepository"/>; never created by handlers (seeded once).
/// </summary>
public sealed class ShopSettings : AggregateRoot, IAuditable
{
    /// <summary>Well-known id of the only row.</summary>
    public static readonly Guid SingletonId = new("00000000-0000-0000-0000-000000000001");

    public const int MaxNameLength = 100;
    public const int MaxPhoneLength = 20;
    public const int MaxAddressLength = 300;
    public const int MaxBankFieldLength = 100;
    public const int MinUnpaidExpiryHours = 1;
    public const int MaxUnpaidExpiryHours = 168;

    private ShopSettings()
        : base(SingletonId)
    {
    }

    public string ShopName { get; private set; } = default!;

    public string ShopPhone { get; private set; } = default!;

    public string ShopAddress { get; private set; } = default!;

    public Money FlatShippingFee { get; private set; } = default!;

    /// <summary>Orders with subtotal at or above this amount ship free. Null disables free shipping.</summary>
    public Money? FreeShippingThreshold { get; private set; }

    public string BankName { get; private set; } = default!;

    public string BankAccountNumber { get; private set; } = default!;

    public string BankAccountName { get; private set; } = default!;

    /// <summary>NAPAS bank identification number used to generate VietQR payloads.</summary>
    public string BankBin { get; private set; } = default!;

    /// <summary>Unpaid bank-transfer orders are auto-cancelled after this many hours.</summary>
    public int UnpaidBankTransferExpiryHours { get; private set; }

    public DateTimeOffset CreatedAt { get; set; }

    public Guid? CreatedBy { get; set; }

    public DateTimeOffset? UpdatedAt { get; set; }

    public Guid? UpdatedBy { get; set; }

    /// <summary>Default values used by the seeder. Admin must replace them before go-live.</summary>
    public static ShopSettings CreateDefault() => new()
    {
        ShopName = "SnackShop",
        ShopPhone = "0900000000",
        ShopAddress = "Chưa cấu hình",
        FlatShippingFee = Money.Vnd(25_000m),
        FreeShippingThreshold = Money.Vnd(300_000m),
        BankName = "Chưa cấu hình",
        BankAccountNumber = "0000000000",
        BankAccountName = "CHUA CAU HINH",
        BankBin = "970000",
        UnpaidBankTransferExpiryHours = 24,
    };

    public Result Update(
        string shopName,
        string shopPhone,
        string shopAddress,
        Money flatShippingFee,
        Money? freeShippingThreshold,
        string bankName,
        string bankAccountNumber,
        string bankAccountName,
        string bankBin,
        int unpaidBankTransferExpiryHours)
    {
        ArgumentNullException.ThrowIfNull(flatShippingFee);

        if (freeShippingThreshold is not null && freeShippingThreshold.IsZero)
        {
            return Result.Failure(SettingsErrors.FreeShippingThresholdMustBePositive);
        }

        if (freeShippingThreshold is not null && freeShippingThreshold <= flatShippingFee)
        {
            return Result.Failure(SettingsErrors.FreeShippingThresholdTooLow);
        }

        if (unpaidBankTransferExpiryHours is < MinUnpaidExpiryHours or > MaxUnpaidExpiryHours)
        {
            return Result.Failure(SettingsErrors.UnpaidExpiryOutOfRange);
        }

        ShopName = shopName.Trim();
        ShopPhone = shopPhone.Trim();
        ShopAddress = shopAddress.Trim();
        FlatShippingFee = flatShippingFee;
        FreeShippingThreshold = freeShippingThreshold;
        BankName = bankName.Trim();
        BankAccountNumber = bankAccountNumber.Trim();
        BankAccountName = bankAccountName.Trim().ToUpperInvariant();
        BankBin = bankBin.Trim();
        UnpaidBankTransferExpiryHours = unpaidBankTransferExpiryHours;

        return Result.Success();
    }

    /// <summary>Shipping fee for a given order subtotal under the current policy.</summary>
    public Money CalculateShippingFee(Money subtotal)
    {
        ArgumentNullException.ThrowIfNull(subtotal);

        if (FreeShippingThreshold is not null && subtotal >= FreeShippingThreshold)
        {
            return Money.Zero(subtotal.Currency);
        }

        return FlatShippingFee;
    }
}
