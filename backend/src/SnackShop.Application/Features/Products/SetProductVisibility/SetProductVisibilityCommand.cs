using SnackShop.Application.Abstractions.Messaging;

namespace SnackShop.Application.Features.Products.SetProductVisibility;

public sealed record SetProductVisibilityCommand(Guid Id, bool IsActive, bool IsFeatured) : ICommand;
