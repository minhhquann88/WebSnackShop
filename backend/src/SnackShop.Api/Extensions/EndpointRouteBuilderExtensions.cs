using SnackShop.Api.Endpoints;

namespace SnackShop.Api.Extensions;

public static class EndpointRouteBuilderExtensions
{
    /// <summary>Instantiates every <see cref="IEndpointGroup"/> in the Api assembly and maps it. Runs once at startup.</summary>
    public static IEndpointRouteBuilder MapEndpointGroups(this IEndpointRouteBuilder app)
    {
        var groups = typeof(IEndpointGroup).Assembly
            .GetTypes()
            .Where(t => t is { IsAbstract: false, IsInterface: false } && typeof(IEndpointGroup).IsAssignableFrom(t))
            .OrderBy(t => t.FullName, StringComparer.Ordinal)
            .Select(t => (IEndpointGroup)Activator.CreateInstance(t)!);

        foreach (var group in groups)
        {
            group.Map(app);
        }

        return app;
    }
}
