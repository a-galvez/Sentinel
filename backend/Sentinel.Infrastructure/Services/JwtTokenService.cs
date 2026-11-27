using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Sentinel.Application.Abstractions;
using Sentinel.Application.Abstractions.Repositories;
using Sentinel.Domain.Entities;

namespace Sentinel.Infrastructure.Services;

// Emite el JWT (access token) y prepara el refresh token para que el caso de uso lo persista.
// No llama SaveChanges: el caso de uso decide cuándo confirmar (login, refresh o registro atómico).
public sealed class JwtTokenService(
    IRefreshTokenRepository refreshTokens,
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

    private Task<AuthTokens> EmitirConAsync(Usuario usuario, Guid? reemplazaPorId, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        var ahora = clock.UtcAhora;
        var expiraEn = ahora.Add(AccesoDuracion);

        var accessToken = GenerarAccessToken(usuario, ahora, expiraEn);
        var refreshToken = GenerarRefreshToken();

        // Solo se deja en el ChangeTracker; el caso de uso confirma con IUnitOfWork.
        refreshTokens.Agregar(new RefreshToken
        {
            RefreshTokenId = Guid.NewGuid(),
            UsuarioId = usuario.UsuarioId,
            TokenHash = HashSha256(refreshToken),
            Expira = ahora.Add(RefreshDuracion),
            ReemplazadoPorId = reemplazaPorId
        });

        return Task.FromResult(new AuthTokens(accessToken, refreshToken, expiraEn));
    }

    private string GenerarAccessToken(Usuario usuario, DateTime ahora, DateTime expira)
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
            notBefore: ahora,
            expires: expira,
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
