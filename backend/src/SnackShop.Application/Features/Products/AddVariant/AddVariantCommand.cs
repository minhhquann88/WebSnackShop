using SnackShop.Application.Abstractions.Messaging;
using SnackShop.Application.Features.Products.Common;

namespace SnackShop.Application.Features.Products.AddVariant;

/// <summary>Adds a variant (and its stock record) to a product. Returns the new variant id.</summary>
public sealed record AddVariantCommand(Guid ProductId, VariantRequest Variant, int InitialStock = 0) : ICommand<Guid>;
