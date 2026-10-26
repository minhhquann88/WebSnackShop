using SnackShop.Application.Common;
using SnackShop.Application.Features.Products.Common;

namespace SnackShop.Api.Contracts;

// Request bodies for endpoints whose identifiers come from the route.
// Commands are built in the endpoint as `new XCommand(routeId, body...)`.

public sealed record UpdateCategoryRequest(string Name, string? Slug, string? Description, string? ImageUrl, int SortOrder, bool IsActive);

public sealed record UpdateProductRequest(
    Guid CategoryId,
    string Name,
    string? Slug,
    string? ShortDescription,
    string? Description,
    string? Ingredients,
    string? Origin,
    string? Brand,
    int? ShelfLifeDays);

public sealed record ProductVisibilityRequest(bool IsActive, bool IsFeatured);

public sealed record AddVariantRequest(VariantRequest Variant, int InitialStock = 0);

public sealed record UpdateVariantRequest(VariantRequest Variant, bool IsActive);

public sealed record ReorderRequest(IReadOnlyList<Guid> Ids);

public sealed record ReceiveStockRequest(int Quantity, string? Note);

public sealed record AdjustStockRequest(int Delta, string Reason);

public sealed record ThresholdRequest(int Threshold);

public sealed record ConfirmOrderRequest(bool MarkPaid = false, string? TransactionRef = null);

public sealed record ShipOrderRequest(string? Note);

public sealed record CancelRequest(string? Reason);

public sealed record MarkPaidRequest(string? TransactionRef, DateTimeOffset? PaidAt);

public sealed record InternalNoteRequest(string? Note);

public sealed record RefundRequest(string? TransactionRef);

public sealed record SetRolesRequest(IReadOnlyList<string> Roles);

public sealed record SetLockRequest(bool Locked);

public sealed record SaveAddressRequest(string? Label, AddressRequest Address, bool IsDefault = false);

public sealed record CartQuantityRequest(int Quantity);

public sealed record RefreshTokenRequest(string RefreshToken);

/// <summary>Checkout body. The idempotency key travels only in the Idempotency-Key header.</summary>
public sealed record PlaceOrderRequest(
    IReadOnlyList<SnackShop.Application.Features.Cart.CartLineRequest> Items,
    string ContactEmail,
    AddressRequest ShippingAddress,
    SnackShop.Domain.Ordering.PaymentMethod PaymentMethod,
    string? CustomerNote);
