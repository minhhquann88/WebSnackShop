using SnackShop.Application.Abstractions.Messaging;

namespace SnackShop.Application.Features.Me.ChangePassword;

/// <summary>
/// Changes (or, for Google-only accounts, sets) the password and signs out every session.
/// <see cref="CurrentPassword"/> is ignored when the account has no password yet.
/// </summary>
public sealed record ChangePasswordCommand(string? CurrentPassword, string NewPassword) : ICommand;
