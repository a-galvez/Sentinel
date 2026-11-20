using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Sentinel.Application.Abstractions;
using Sentinel.Domain.Entities;

namespace Sentinel.Infrastructure.Services;

// Emite el JWT (access token) y gestiona los refresh tokens: los guarda con hash SHA256
// (determinístico, para buscarlos en POST /auth/refresh) y devuelve el valor en claro.
public sealed class JwtTokenService(
    IAppDbContext db,
    IClock clock,
    IConfiguration cfg) : ITokenService
{
    // Duraciones configurables (x-supuesto H2): access token 15 min, refresh token 7 días.
    private TimeSpan AccesoDuracion => TimeSpan.FromMinutes(cfg.GetValue("Jwt:AccesoMinutos", 15));
    private TimeSpan RefreshDuracion => TimeSpan.FromDays(cfg.GetValue("Jwt:RefreshDias", 7));

    public Task<AuthTokens> EmitirAsync(Usuario usuario, CancellationToken ct)
        => EmitirConAsync(usuario, reemplazaPorId: null, ct);

    public Task<AuthTokens> RotarAsync(Usuario usuario, Guid reemplazaA, CancellationToken ct)
        => EmitirConAsync(usuario, reemplazaPorId: reemplazaA, ct);

    public string HashRefreshToken(string refreshToken) => HashSha256(refreshToken);

    private async Task<AuthTokens> EmitirConAsync(Usuario usuario, Guid? reemplazaPorId, CancellationToken ct)
    {
        var accessToken = GenerarAccessToken(usuario);

        var refreshToken = GenerarRefreshToken();
        var token = new RefreshToken
        {
            RefreshTokenId = Guid.NewGuid(),
            UsuarioId = usuario.UsuarioId,
            TokenHash = HashSha256(refreshToken),
            Expira = clock.UtcAhora.Add(RefreshDuracion),
            ReemplazadoPorId = reemplazaPorId
        };

        db.RefreshTokens.Add(token);
        await db.SaveChangesAsync(ct);

        return new AuthTokens(accessToken, refreshToken);
    }

    private string GenerarAccessToken(Usuario usuario)
    {
        var secret = cfg["Jwt:Secret"] ?? cfg["JWT__Secret"]
                     ?? throw new InvalidOperationException("Falta la variable JWT__Secret.");
        var issuer = cfg["Jwt:Issuer"] ?? cfg["JWT__Issuer"] ?? "sentinel";
        var audience = cfg["Jwt:Audience"] ?? cfg["JWT__Audience"] ?? "sentinel-web";

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
        var signing = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        // El JWT lleva `sub` (usuario) y `empresaId` como claims; el backend filtra por esta última.
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, usuario.UsuarioId.ToString()),
            new("empresaId", usuario.EmpresaId.ToString())
        };

        var jwt = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            notBefore: clock.UtcAhora,
            expires: clock.UtcAhora.Add(AccesoDuracion),
            signingCredentials: signing);

        return new JwtSecurityTokenHandler().WriteToken(jwt);
    }

    private static string GenerarRefreshToken()
        => Convert.ToBase64String(RandomNumberGenerator.GetBytes(48))
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');

    private static string HashSha256(string valor)
        => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(valor)));
}
