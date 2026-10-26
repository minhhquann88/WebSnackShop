using SnackShop.Application.Abstractions.Messaging;

namespace SnackShop.Application.Features.Users.SetUserLock;

/// <summary>Disables (or re-enables) an account. Locking signs the user out everywhere.</summary>
public sealed record SetUserLockCommand(Guid UserId, bool Locked) : ICommand;
