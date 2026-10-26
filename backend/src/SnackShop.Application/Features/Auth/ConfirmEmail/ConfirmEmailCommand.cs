using SnackShop.Application.Abstractions.Messaging;

namespace SnackShop.Application.Features.Auth.ConfirmEmail;

public sealed record ConfirmEmailCommand(Guid UserId, string Token) : ICommand;
