using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Sentinel.Tests.Helpers;

// Autenticación de prueba: toma los ids de las cabeceras y los pone como claims del JWT real
// (empresaId + sub), sin necesidad de firmar un token. Úsalo sólo en pruebas.
public sealed class TestAuthHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder)
    : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    public new const string Scheme = "TestAuth";
    public const string EmpresaHeader = "X-Test-Empresa";
    public const string UsuarioHeader = "X-Test-Usuario";

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var empresa = Request.Headers[EmpresaHeader].ToString();
        if (string.IsNullOrWhiteSpace(empresa))
            return Task.FromResult(AuthenticateResult.NoResult());

        var usuario = Request.Headers[UsuarioHeader].ToString();
        var claims = new List<Claim>
        {
            new("empresaId", empresa),
            new(ClaimTypes.NameIdentifier, string.IsNullOrWhiteSpace(usuario) ? empresa : usuario)
        };
        if (!string.IsNullOrWhiteSpace(usuario))
            claims.Add(new Claim("sub", usuario));

        var identity = new ClaimsIdentity(claims, Scheme);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, Scheme);
        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
