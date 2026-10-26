using SnackShop.Application.Abstractions.Messaging;
using SnackShop.Application.Features.Auth;

namespace SnackShop.Application.Features.Me.GetMe;

/// <summary>The signed-in user's account (used by the frontend after sign-in and on page load).</summary>
public sealed record GetMeQuery : IQuery<UserResponse>;
