using System.Reflection;
using FluentValidation;
using MediatR;
using NetArchTest.Rules;
using Shouldly;
using SnackShop.Application.Abstractions.Messaging;
using SnackShop.Domain.Common;

namespace SnackShop.ArchitectureTests;

/// <summary>Machine-checkable parts of docs/04-coding-conventions.md.</summary>
public sealed class ConventionTests
{
    private static readonly Assembly Domain = typeof(Result).Assembly;
    private static readonly Assembly Application = typeof(SnackShop.Application.DependencyInjection).Assembly;

    [Fact]
    public void Handlers_AreSealed_AndEndWithHandler()
    {
        var handlerTypes = Application.GetTypes()
            .Where(t => t is { IsClass: true, IsAbstract: false } && t.GetInterfaces().Any(i =>
                i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IRequestHandler<,>)))
            .ToList();

        handlerTypes.ShouldNotBeEmpty();
        handlerTypes.Where(t => !t.IsSealed).Select(t => t.Name).ShouldBeEmpty("Handlers must be sealed");
        handlerTypes.Where(t => !t.Name.EndsWith("Handler", StringComparison.Ordinal)).Select(t => t.Name).ShouldBeEmpty();
    }

    [Fact]
    public void EveryCommand_HasAValidator()
    {
        var commandTypes = Application.GetTypes()
            .Where(t => t is { IsClass: true, IsAbstract: false } && typeof(IBaseCommand).IsAssignableFrom(t))
            .ToList();

        var validatedTypes = Application.GetTypes()
            .Where(t => t is { IsClass: true, IsAbstract: false })
            .SelectMany(t => t.GetInterfaces())
            .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IValidator<>))
            .Select(i => i.GetGenericArguments()[0])
            .ToHashSet();

        commandTypes.ShouldNotBeEmpty();
        commandTypes.Where(c => !validatedTypes.Contains(c)).Select(c => c.Name)
            .ShouldBeEmpty("Every ICommand needs an AbstractValidator<TCommand>");
    }

    [Fact]
    public void Commands_And_Queries_AreRecords_AndImmutable()
    {
        var requestTypes = Application.GetTypes()
            .Where(t => t is { IsClass: true, IsAbstract: false } && t.GetInterfaces().Any(i =>
                i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IRequest<>)))
            .ToList();

        requestTypes.ShouldNotBeEmpty();
        foreach (var type in requestTypes)
        {
            type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.SetMethod is { IsPublic: true } && !IsInitOnly(p))
                .Select(p => $"{type.Name}.{p.Name}")
                .ShouldBeEmpty("Requests must be immutable (records with init-only properties)");
        }
    }

    [Fact]
    public void DomainEntities_HaveNoPublicSetters_ExceptAuditColumns()
    {
        var auditMembers = typeof(IAuditable).GetProperties().Select(p => p.Name)
            .Concat(typeof(ISoftDeletable).GetProperties().Select(p => p.Name))
            .ToHashSet(StringComparer.Ordinal);

        var entityTypes = Domain.GetTypes()
            .Where(t => t is { IsClass: true, IsAbstract: false } && typeof(Entity).IsAssignableFrom(t));

        var offenders = entityTypes
            .SelectMany(t => t.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.SetMethod is { IsPublic: true } && !IsInitOnly(p) && !auditMembers.Contains(p.Name))
                .Select(p => $"{t.Name}.{p.Name}"))
            .ToList();

        offenders.ShouldBeEmpty("Entity state changes only through methods");
    }

    [Fact]
    public void DomainEntities_AreSealed()
    {
        var result = Types.InAssembly(Domain)
            .That().Inherit(typeof(Entity))
            .And().AreNotAbstract()
            .Should().BeSealed()
            .GetResult();

        result.IsSuccessful.ShouldBeTrue(LayerDependencyTests.Describe(result));
    }

    [Fact]
    public void Validators_EndWithValidator()
    {
        var offenders = Application.GetTypes()
            .Where(t => t is { IsClass: true, IsAbstract: false } && typeof(IValidator).IsAssignableFrom(t))
            .Where(t => !t.Name.EndsWith("Validator", StringComparison.Ordinal))
            .Select(t => t.Name)
            .ToList();

        offenders.ShouldBeEmpty();
    }

    private static bool IsInitOnly(PropertyInfo property) =>
        property.SetMethod?.ReturnParameter.GetRequiredCustomModifiers()
            .Any(m => m.FullName == "System.Runtime.CompilerServices.IsExternalInit") ?? false;
}
