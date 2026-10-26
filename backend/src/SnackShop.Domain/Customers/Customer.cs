using SnackShop.Domain.Common;

namespace SnackShop.Domain.Customers;

/// <summary>
/// Shopping profile of a registered user. Shares its Id with the identity account (owned by infrastructure).
/// Owns the address book: at most <see cref="MaxAddresses"/> entries, exactly one default when any exist.
/// </summary>
public sealed class Customer : AggregateRoot, IAuditable
{
    public const int MaxAddresses = 5;
    public const int MaxNameLength = 100;
    public const int MaxEmailLength = 256;

    private readonly List<Address> _addresses = [];

    private Customer()
    {
    }

    private Customer(Guid id, string fullName, string email, string? phone)
        : base(id)
    {
        FullName = fullName;
        Email = email;
        Phone = phone;
    }

    public string FullName { get; private set; } = default!;

    public string Email { get; private set; } = default!;

    public string? Phone { get; private set; }

    public IReadOnlyCollection<Address> Addresses => _addresses.AsReadOnly();

    public DateTimeOffset CreatedAt { get; set; }

    public Guid? CreatedBy { get; set; }

    public DateTimeOffset? UpdatedAt { get; set; }

    public Guid? UpdatedBy { get; set; }

    /// <summary>Creates the profile for a new account. <paramref name="userId"/> is the identity account Id, reused as the customer Id.</summary>
    public static Customer Create(Guid userId, string fullName, string email, string? phone) =>
        new(userId, fullName.Trim(), email.Trim().ToLowerInvariant(), Clean(phone));

    public void UpdateProfile(string fullName, string? phone)
    {
        FullName = fullName.Trim();
        Phone = Clean(phone);
    }

    public Result<Address> AddAddress(string? label, AddressInfo info, bool makeDefault)
    {
        ArgumentNullException.ThrowIfNull(info);

        if (_addresses.Count >= MaxAddresses)
        {
            return Result.Failure<Address>(CustomerErrors.TooManyAddresses);
        }

        var isDefault = makeDefault || _addresses.Count == 0;
        var address = new Address(Id, label, info, isDefault);

        if (isDefault)
        {
            _addresses.ForEach(a => a.SetDefault(false));
        }

        _addresses.Add(address);
        return address;
    }

    public Result UpdateAddress(Guid addressId, string? label, AddressInfo info)
    {
        ArgumentNullException.ThrowIfNull(info);

        var address = _addresses.Find(a => a.Id == addressId);
        if (address is null)
        {
            return Result.Failure(CustomerErrors.AddressNotFound);
        }

        address.Apply(label, info);
        return Result.Success();
    }

    public Result RemoveAddress(Guid addressId)
    {
        var address = _addresses.Find(a => a.Id == addressId);
        if (address is null)
        {
            return Result.Failure(CustomerErrors.AddressNotFound);
        }

        _addresses.Remove(address);
        if (address.IsDefault && _addresses.Count > 0)
        {
            _addresses[0].SetDefault(true);
        }

        return Result.Success();
    }

    public Result SetDefaultAddress(Guid addressId)
    {
        if (!_addresses.Exists(a => a.Id == addressId))
        {
            return Result.Failure(CustomerErrors.AddressNotFound);
        }

        _addresses.ForEach(a => a.SetDefault(a.Id == addressId));
        return Result.Success();
    }

    private static string? Clean(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
