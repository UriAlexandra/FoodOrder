using FoodOrder.Domain.Entities;
using NetArchTest.Rules;

namespace FoodOrder.Api.Tests.Architecture;

/// <summary>A Clean Architecture függőségi szabályai: a belső rétegek nem ismerik a külsőket.</summary>
public class ArchitectureTests
{
    private static readonly System.Reflection.Assembly Domain = typeof(Order).Assembly;
    private static readonly System.Reflection.Assembly Application = typeof(FoodOrder.Application.DependencyInjection).Assembly;
    private static readonly System.Reflection.Assembly Client = typeof(FoodOrder.Client._Imports).Assembly;

    [Fact]
    public void Domain_DoesNotDependOnOtherLayersOrEfCore()
    {
        var result = Types.InAssembly(Domain)
            .ShouldNot()
            .HaveDependencyOnAny("FoodOrder.Application", "FoodOrder.Infrastructure", "FoodOrder.Shared", "Microsoft.EntityFrameworkCore")
            .GetResult();

        Assert.True(result.IsSuccessful, Describe(result));
    }

    [Fact]
    public void Application_DoesNotDependOnInfrastructureOrEfCore()
    {
        var result = Types.InAssembly(Application)
            .ShouldNot()
            .HaveDependencyOnAny("FoodOrder.Infrastructure", "Microsoft.EntityFrameworkCore")
            .GetResult();

        Assert.True(result.IsSuccessful, Describe(result));
    }

    [Fact]
    public void Application_DoesNotDependOnAspNetCore()
    {
        var result = Types.InAssembly(Application)
            .ShouldNot()
            .HaveDependencyOn("Microsoft.AspNetCore")
            .GetResult();

        Assert.True(result.IsSuccessful, Describe(result));
    }

    [Fact]
    public void Client_DoesNotReferenceDomainApplicationOrInfrastructure()
    {
        var result = Types.InAssembly(Client)
            .ShouldNot()
            .HaveDependencyOnAny("FoodOrder.Domain", "FoodOrder.Application", "FoodOrder.Infrastructure")
            .GetResult();

        Assert.True(result.IsSuccessful, Describe(result));
    }

    private static string Describe(TestResult result) =>
        "Szabálysértő típusok: " + string.Join(", ", result.FailingTypeNames ?? []);
}
