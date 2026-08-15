using NetArchTest.Rules;
using OniBusExpress.Domain.Passengers;

namespace OniBusExpress.ArchitectureTests;

public class LayeringTests
{
    private const string Domain = "OniBusExpress.Domain";
    private const string Application = "OniBusExpress.Application";
    private const string Infrastructure = "OniBusExpress.Infrastructure";
    private const string Api = "OniBusExpress.Api";

    private static readonly System.Reflection.Assembly DomainAssembly = typeof(Cpf).Assembly;

    [Fact]
    public void Dominio_NaoDependeDeNenhumaOutraCamada()
    {
        var result = Types.InAssembly(DomainAssembly)
            .Should()
            .NotHaveDependencyOnAny(Application, Infrastructure, Api)
            .GetResult();

        Assert.True(result.IsSuccessful, Describe(result));
    }

    [Fact]
    public void Aplicacao_NaoDependeDeInfraestruturaNemDaApi()
    {
        var result = Types.InAssembly(typeof(Application.ApplicationServices).Assembly)
            .Should()
            .NotHaveDependencyOnAny(Infrastructure, Api)
            .GetResult();

        Assert.True(result.IsSuccessful, Describe(result));
    }

    [Fact]
    public void Infraestrutura_NaoDependeDaApi()
    {
        var result = Types.InAssembly(typeof(Infrastructure.InfrastructureServices).Assembly)
            .Should()
            .NotHaveDependencyOn(Api)
            .GetResult();

        Assert.True(result.IsSuccessful, Describe(result));
    }

    private static string Describe(TestResult result) =>
        result.IsSuccessful
            ? string.Empty
            : "Tipos que violam a fronteira: " + string.Join(", ", result.FailingTypeNames);
}
