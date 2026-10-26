using SnackShop.Domain.Common;

namespace SnackShop.Domain.Customers;

/// <summary>A saved delivery address in a customer's address book. Part of the Customer aggregate.</summary>
public sealed class Address : Entity
{
    public const int MaxLabelLength = 50;

    private Address()
    {
    }

    internal Address(Guid customerId, string? label, AddressInfo info, bool isDefault)
        : base(Guid.CreateVersion7())
    {
        CustomerId = customerId;
        Apply(label, info);
        IsDefault = isDefault;
    }

    public Guid CustomerId { get; private set; }

    public string? Label { get; private set; }

    public string FullName { get; private set; } = default!;

    public string Phone { get; private set; } = default!;

    public string Line1 { get; private set; } = default!;

    public string Ward { get; private set; } = default!;

    public string District { get; private set; } = default!;

    public string Province { get; private set; } = default!;

    public bool IsDefault { get; private set; }

    public AddressInfo ToInfo() => new(FullName, Phone, Line1, Ward, District, Province);

    internal void Apply(string? label, AddressInfo info)
    {
        var normalized = info.Normalize();
        Label = string.IsNullOrWhiteSpace(label) ? null : label.Trim();
        FullName = normalized.FullName;
        Phone = normalized.Phone;
        Line1 = normalized.Line1;
        Ward = normalized.Ward;
        District = normalized.District;
        Province = normalized.Province;
    }

    internal void SetDefault(bool isDefault) => IsDefault = isDefault;
}
