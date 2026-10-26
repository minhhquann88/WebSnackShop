namespace SnackShop.Api.Endpoints;

/// <summary>
/// One implementation per module. Discovered and mapped automatically by MapEndpointGroups().
/// Endpoints only bind the request, send a command/query, and map the Result to HTTP.
/// </summary>
public interface IEndpointGroup
{
    void Map(IEndpointRouteBuilder app);
}
