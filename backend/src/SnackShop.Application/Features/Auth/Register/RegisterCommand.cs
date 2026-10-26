using SnackShop.Application.Abstractions.Messaging;

namespace SnackShop.Application.Features.Auth.Register;

/// <summary>Creates a customer account (and its shopping profile) and signs it in.</summary>
public sealed record RegisterCommand(string Email, string Password, string FullName, string? PhoneNumber) : ICommand<AuthResponse>;
