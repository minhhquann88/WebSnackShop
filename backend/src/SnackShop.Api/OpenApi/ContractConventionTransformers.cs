using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;
using SnackShop.Application.Common;

namespace SnackShop.Api.OpenApi;

/// <summary>
/// Query parameters bound with [AsParameters] from C# records come out PascalCase; the API convention is camelCase
/// (docs/05 §1). Binding is case-insensitive, so only the documented name changes.
/// </summary>
internal sealed class CamelCaseQueryParametersTransformer : IOpenApiOperationTransformer
{
    public Task TransformAsync(OpenApiOperation operation, OpenApiOperationTransformerContext context, CancellationToken cancellationToken)
    {
        foreach (var parameter in (operation.Parameters ?? []).OfType<OpenApiParameter>())
        {
            if (parameter.In == ParameterLocation.Query && parameter.Name is { Length: > 0 } name && char.IsUpper(name[0]))
            {
                parameter.Name = char.ToLowerInvariant(name[0]) + name[1..];
            }
        }

        return Task.CompletedTask;
    }
}

/// <summary>Computed paging properties are always serialized; mark them required so generated clients are not optional.</summary>
internal sealed class PagedResponseSchemaTransformer : IOpenApiSchemaTransformer
{
    private static readonly string[] AlwaysPresent = ["totalPages", "hasNextPage", "hasPreviousPage"];

    public Task TransformAsync(OpenApiSchema schema, OpenApiSchemaTransformerContext context, CancellationToken cancellationToken)
    {
        var type = context.JsonTypeInfo.Type;
        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(PagedResponse<>))
        {
            schema.Required ??= new HashSet<string>(StringComparer.Ordinal);
            foreach (var name in AlwaysPresent)
            {
                schema.Required.Add(name);
            }
        }

        return Task.CompletedTask;
    }
}
