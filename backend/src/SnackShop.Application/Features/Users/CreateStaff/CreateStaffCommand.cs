using SnackShop.Application.Abstractions.Messaging;

namespace SnackShop.Application.Features.Users.CreateStaff;

/// <summary>Creates an employee account with a temporary password. <see cref="IsAdmin"/> also grants the Admin role.</summary>
public sealed record CreateStaffCommand(string Email, string FullName, string Password, bool IsAdmin) : ICommand<Guid>;
