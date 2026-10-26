using SnackShop.Application.Abstractions.Messaging;

namespace SnackShop.Application.Features.Auth.ForgotPassword;

/// <summary>Emails a reset link when the account exists. Always succeeds so emails cannot be enumerated.</summary>
public sealed record ForgotPasswordCommand(string Email) : ICommand;
