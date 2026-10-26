namespace SnackShop.Application.Abstractions.Services;

/// <summary>The authenticated principal of the current request, or anonymous.</summary>
public interface ICurrentUser
{
    bool IsAuthenticated { get; }

    /// <summary>Null when anonymous.</summary>
    Guid? UserId { get; }

    string? Email { get; }

    IReadOnlyCollection<string> Roles { get; }

    bool IsInRole(string role);
}
