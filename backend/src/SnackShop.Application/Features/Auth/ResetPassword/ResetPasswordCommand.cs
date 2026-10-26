using SnackShop.Application.Abstractions.Messaging;

namespace SnackShop.Application.Features.Auth.ResetPassword;

/// <summary>Sets a new password from an emailed token and signs out every session of the account.</summary>
public sealed record ResetPasswordCommand(string Email, string Token, string NewPassword) : ICommand;
