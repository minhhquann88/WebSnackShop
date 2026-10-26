using SnackShop.Application.Abstractions.Messaging;
using SnackShop.Application.Features.Products.Common;

namespace SnackShop.Application.Features.Products.UpdateVariant;

public sealed record UpdateVariantCommand(Guid ProductId, Guid VariantId, VariantRequest Variant, bool IsActive) : ICommand;
