using SnackShop.Domain.Common;

namespace SnackShop.Application.Common;

/// <summary>Authentication errors. Messages are deliberately generic so they never reveal whether an email exists.</summary>
public static class AuthErrors
{
    public static readonly Error InvalidCredentials = Error.Unauthorized("Auth.InvalidCredentials", "Email or password is incorrect.");

    public static readonly Error LockedOut = Error.Unauthorized(
        "Auth.LockedOut", "The account is temporarily locked after too many failed attempts, or has been disabled.");

    public static readonly Error EmailTaken = Error.Conflict("Auth.EmailTaken", "An account with this email already exists.");

    public static readonly Error InvalidRefreshToken = Error.Unauthorized("Auth.InvalidRefreshToken", "The session has expired. Please sign in again.");

    public static readonly Error RefreshTokenReused = Error.Unauthorized(
        "Auth.RefreshTokenReused", "This session was used elsewhere and has been closed for safety. Please sign in again.");

    public static readonly Error InvalidToken = Error.Validation("Auth.InvalidToken", "The link is invalid or has expired.");

    public static readonly Error WrongPassword = Error.Validation("Auth.WrongPassword", "The current password is incorrect.");

    public static readonly Error UserNotFound = Error.NotFound("Auth.UserNotFound", "The user was not found.");

    public static readonly Error GoogleEmailNotVerified = Error.Unauthorized(
        "Auth.GoogleEmailNotVerified", "The Google account email is not verified.");

    public static readonly Error CannotChangeOwnRoles = Error.Conflict(
        "Auth.CannotChangeOwnRoles", "You cannot remove your own admin role or lock your own account.");

    public static readonly Error InvalidRole = Error.Validation("Auth.InvalidRole", "Unknown role.");

    public static Error PasswordRejected(IReadOnlyList<string> reasons) =>
        ValidationError.FromSingle("password", string.Join(" ", reasons));
}
