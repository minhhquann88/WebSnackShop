namespace SnackShop.Application.Common.Authorization;

/// <summary>Authorization policy names. Defined in Api (AddAuthorizationPolicies), referenced by endpoints.</summary>
public static class Policies
{
    /// <summary>Any authenticated user.</summary>
    public const string Customer = "Customer";

    /// <summary>Role Staff or Admin.</summary>
    public const string Staff = "Staff";

    /// <summary>Role Admin only.</summary>
    public const string Admin = "Admin";
}
