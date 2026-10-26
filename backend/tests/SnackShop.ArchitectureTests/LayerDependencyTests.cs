using System.Reflection;
using NetArchTest.Rules;
using Shouldly;
using SnackShop.Domain.Common;

namespace SnackShop.ArchitectureTests;

/// <summary>Enforces the dependency rule from docs/02-architecture.md §2.</summary>
public sealed class LayerDependencyTests
{
    private static readonly Assembly Domain = typeof(Result).Assembly;
    private static readonly Assembly Application = typeof(SnackShop.Application.DependencyInjection).Assembly;
    private static readonly Assembly Infrastructure = typeof(SnackShop.Infrastructure.DependencyInjection).Assembly;
    private static readonly Assembly Api = typeof(Program).Assembly;

    private const string ApplicationNs = "SnackShop.Application";
    private const string InfrastructureNs = "SnackShop.Infrastructure";
    private const string ApiNs = "SnackShop.Api";

    [Fact]
    public void Domain_DependsOnNothingButBcl()
    {
        var result = Types.InAssembly(Domain)
            .ShouldNot()
            .HaveDependencyOnAny(
                ApplicationNs,
                InfrastructureNs,
                ApiNs,
                "Microsoft.EntityFrameworkCore",
                "Microsoft.AspNetCore",
                "MediatR",
                "FluentValidation",
                "Npgsql")
            .GetResult();

        result.IsSuccessful.ShouldBeTrue(Describe(result));
    }

    [Fact]
    public void Application_DoesNotDependOnOuterLayers()
    {
        var result = Types.InAssembly(Application)
            .ShouldNot()
            .HaveDependencyOnAny(InfrastructureNs, ApiNs, "Microsoft.AspNetCore", "Npgsql", "AutoMapper")
            .GetResult();

        result.IsSuccessful.ShouldBeTrue(Describe(result));
    }

    [Fact]
    public void Infrastructure_DoesNotDependOnApi()
    {
        var result = Types.InAssembly(Infrastructure)
            .ShouldNot()
            .HaveDependencyOnAny(ApiNs, "AutoMapper")
            .GetResult();

        result.IsSuccessful.ShouldBeTrue(Describe(result));
    }

    [Fact]
    public void Api_DoesNotUseDbContextDirectly()
    {
        var result = Types.InAssembly(Api)
            .That().ResideInNamespaceStartingWith(ApiNs + ".Endpoints")
            .ShouldNot()
            .HaveDependencyOnAny("Microsoft.EntityFrameworkCore", InfrastructureNs + ".Persistence")
            .GetResult();

        result.IsSuccessful.ShouldBeTrue(Describe(result));
    }

    [Fact]
    public void Api_HasNoControllers()
    {
        var result = Types.InAssembly(Api)
            .Should()
            .NotInherit(typeof(Microsoft.AspNetCore.Mvc.ControllerBase))
            .GetResult();

        result.IsSuccessful.ShouldBeTrue(Describe(result));
    }

    internal static string Describe(NetArchTest.Rules.TestResult result) =>
        result.IsSuccessful
            ? string.Empty
            : "Violations: " + string.Join(", ", result.FailingTypeNames ?? []);
}
