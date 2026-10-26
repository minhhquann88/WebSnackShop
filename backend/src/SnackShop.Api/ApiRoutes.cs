namespace SnackShop.Api;

public static class ApiRoutes
{
    public const string V1 = "/api/v1";
    public const string AdminV1 = V1 + "/admin";
    public const string MeV1 = V1 + "/me";
}

public static class RateLimitPolicies
{
    public const string Auth = "auth";
    public const string Session = "session";
    public const string PlaceOrder = "place-order";
}
