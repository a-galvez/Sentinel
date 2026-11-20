using Microsoft.EntityFrameworkCore;
using Sentinel.Application.Abstractions;
using Sentinel.Application.Common.Exceptions;

namespace Sentinel.Application.Features.Auth.Login;

public sealed class LoginHandler(
    IAppDbContext db,
    IPasswordHasher passwordHasher,
    ITokenService tokenService)
{
    public async Task<AuthResponse> ManejarAsync(LoginRequest request, CancellationToken ct)
    {
        var correo = request.Correo.Trim();

        // El login es público (sin sesión), así que se ignora el filtro global por empresa.
        var usuario = await db.Usuarios
            .IgnoreQueryFilters()
            .SingleOrDefaultAsync(u => u.Correo == correo, ct);

        // Mismo mensaje para correo inexistente, contraseña errónea y usuario inactivo,
        // para no indicar cuál de las condiciones falló (contrato /auth/login).
        if (usuario is null
            || !usuario.Activo
            || !passwordHasher.Verificar(usuario.PasswordHash, request.Contrasena))
        {
            throw new UnauthorizedAppException("INVALID_CREDENTIALS", "Correo o contraseña incorrectos");
        }

        var tokens = await tokenService.EmitirAsync(usuario, ct);

        return new AuthResponse(
            tokens.AccessToken,
            tokens.RefreshToken,
            "Bearer",
            UsuarioDto.Desde(usuario));
    }
}
