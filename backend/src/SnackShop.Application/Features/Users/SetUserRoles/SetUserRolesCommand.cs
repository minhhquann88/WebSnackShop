using SnackShop.Application.Abstractions.Messaging;

namespace SnackShop.Application.Features.Users.SetUserRoles;

/// <summary>Replaces the roles of an account. Admins cannot remove their own Admin role.</summary>
public sealed record SetUserRolesCommand(Guid UserId, IReadOnlyList<string> Roles) : ICommand;
