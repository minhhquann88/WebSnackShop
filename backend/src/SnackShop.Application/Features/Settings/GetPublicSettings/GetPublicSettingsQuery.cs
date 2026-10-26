using SnackShop.Application.Abstractions.Messaging;

namespace SnackShop.Application.Features.Settings.GetPublicSettings;

/// <summary>Settings the storefront needs: shop contact, shipping policy, bank account for transfers.</summary>
public sealed record GetPublicSettingsQuery : IQuery<PublicSettingsResponse>;
