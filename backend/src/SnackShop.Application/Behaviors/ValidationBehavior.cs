using FluentValidation;
using MediatR;
using SnackShop.Application.Common;
using SnackShop.Domain.Common;

namespace SnackShop.Application.Behaviors;

/// <summary>
/// Runs every registered validator for the request. On failure short-circuits with a <see cref="ValidationError"/>
/// (API maps it to 400 with per-field errors). Property names are camelCased to match JSON.
/// </summary>
public sealed class ValidationBehavior<TRequest, TResponse>(IEnumerable<IValidator<TRequest>> validators)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
    where TResponse : Result
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        if (!validators.Any())
        {
            return await next(cancellationToken);
        }

        var context = new ValidationContext<TRequest>(request);

        var results = await Task.WhenAll(validators.Select(v => v.ValidateAsync(context, cancellationToken)));

        var failures = results
            .SelectMany(r => r.Errors)
            .Where(f => f is not null)
            .GroupBy(f => ToCamelCasePath(f.PropertyName), StringComparer.Ordinal)
            .ToDictionary(g => g.Key, g => g.Select(f => f.ErrorMessage).Distinct(StringComparer.Ordinal).ToArray(), StringComparer.Ordinal);

        if (failures.Count == 0)
        {
            return await next(cancellationToken);
        }

        return ResultFactory.CreateFailure<TResponse>(new ValidationError(failures));
    }

    /// <summary>"ShippingAddress.Phone" → "shippingAddress.phone"; "Items[0].Quantity" → "items[0].quantity".</summary>
    internal static string ToCamelCasePath(string propertyPath)
    {
        if (string.IsNullOrEmpty(propertyPath))
        {
            return propertyPath;
        }

        var segments = propertyPath.Split('.');
        for (var i = 0; i < segments.Length; i++)
        {
            var s = segments[i];
            if (s.Length > 0 && char.IsUpper(s[0]))
            {
                segments[i] = char.ToLowerInvariant(s[0]) + s[1..];
            }
        }

        return string.Join('.', segments);
    }
}
