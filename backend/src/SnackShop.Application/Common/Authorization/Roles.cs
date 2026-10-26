namespace SnackShop.Application.Common.Authorization;

public static class Roles
{
    public const string Customer = "Customer";
    public const string Staff = "Staff";
    public const string Admin = "Admin";

    public static readonly IReadOnlyList<string> All = [Customer, Staff, Admin];
}
