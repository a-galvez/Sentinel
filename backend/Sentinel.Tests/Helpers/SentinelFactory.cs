using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Sentinel.Domain.Entities;
using Sentinel.Tests.Builders;

namespace Sentinel.Tests.Helpers;

// Fábrica de pruebas con autenticación por cabeceras y configuración en memoria.
public sealed class SentinelFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<IConfiguration>();
            services.AddSingleton<IConfiguration>(_ => new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["ConnectionStrings:PostgresConnection"] = "Host=localhost;Database=SentinelTest;Username=test;Password=test",
                    ["CORS_ALLOWED_ORIGINS"] = "http://localhost",
                    ["Jwt:Secret"] = "clave-de-prueba-con-al-menos-32-caracteres!",
                    ["Jwt:Issuer"] = "sentinel-test",
                    ["Jwt:Audience"] = "sentinel-test",
                    ["Jwt:AccesoMinutos"] = "15",
                    ["Jwt:RefreshDias"] = "7",
                    ["MIGRATE_ON_START"] = "false"
                })
                .Build());

            services.AddAuthentication(TestAuthHandler.Scheme)
                    .AddScheme<Microsoft.AspNetCore.Authentication.AuthenticationSchemeOptions, TestAuthHandler>(
                        TestAuthHandler.Scheme, _ => { });
        });
    }

    // Cliente "autenticado como empresa X, usuario Y".
    public HttpClient ClienteAutenticado(Guid empresaId, Guid? usuarioId = null)
    {
        var client = CreateClient();
        client.DefaultRequestHeaders.Add(TestAuthHandler.EmpresaHeader, empresaId.ToString());
        if (usuarioId.HasValue)
            client.DefaultRequestHeaders.Add(TestAuthHandler.UsuarioHeader, usuarioId.Value.ToString());
        return client;
    }

    // Crea dos empresas independientes y devuelve sus respectivos clientes.
    public (Empresa A, Empresa B, HttpClient ClienteA, HttpClient ClienteB) DosEmpresas()
    {
        var a = EmpresaBuilder.Crear("Empresa A");
        var b = EmpresaBuilder.Crear("Empresa B");
        return (a, b, ClienteAutenticado(a.EmpresaId), ClienteAutenticado(b.EmpresaId));
    }
}
