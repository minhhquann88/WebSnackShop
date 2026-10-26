using System.Collections.Concurrent;
using System.Reflection;
using SnackShop.Domain.Common;

namespace SnackShop.Application.Common;

/// <summary>
/// Creates a failed <see cref="Result"/> or <see cref="Result{T}"/> for a generic TResponse.
/// Used by pipeline behaviors that must short-circuit without knowing the concrete result type.
/// </summary>
internal static class ResultFactory
{
    private static readonly ConcurrentDictionary<Type, MethodInfo> FailureMethodCache = new();

    public static TResponse CreateFailure<TResponse>(Error error)
        where TResponse : Result
    {
        var responseType = typeof(TResponse);

        if (responseType == typeof(Result))
        {
            return (TResponse)Result.Failure(error);
        }

        if (!responseType.IsGenericType || responseType.GetGenericTypeDefinition() != typeof(Result<>))
        {
            throw new InvalidOperationException($"Unsupported result type '{responseType.Name}'.");
        }

        var method = FailureMethodCache.GetOrAdd(responseType, static type =>
        {
            var valueType = type.GetGenericArguments()[0];
            return typeof(Result)
                .GetMethods(BindingFlags.Public | BindingFlags.Static)
                .Single(m => m.Name == nameof(Result.Failure) && m.IsGenericMethodDefinition)
                .MakeGenericMethod(valueType);
        });

        return (TResponse)method.Invoke(null, [error])!;
    }
}
