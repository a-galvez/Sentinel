using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Sentinel.Application.Abstractions;
using Sentinel.Application.Abstractions.Repositories;
using Sentinel.Application.Common;
using Sentinel.Application.Common.Exceptions;
using Sentinel.Application.Features.Auth;
using Sentinel.Application.Features.Auth.Login;

namespace Sentinel.API.Controllers.Auth;

[ApiController]
[Route("api/v1/auth")]
public sealed class LoginController(
    IUsuarioRepository usuarios,
    IEmpresaRepository empresas,
    IPasswordHasher passwordHasher,
    ITokenService tokenService,
    IUnitOfWork uow,
    IValidator<LoginRequest> validator) : ControllerBase
{
    [HttpPost("login")]
    public async Task<ActionResult<SesionResponse>> Login(
        [FromBody] LoginRequest request,
        CancellationToken ct)
    {
        await validator.ValidateAndThrowAsync(request, ct);

        // El login es público (sin sesión): el repositorio ignora el filtro global por empresa.
        var correo = Correo.Normalizar(request.Correo);
        var usuario = await usuarios.ObtenerPorCorreoAsync(correo, ct);

        // Mismo mensaje para correo inexistente, contraseña errónea y usuario inactivo,
        // para no indicar cuál de las condiciones falló (contrato /auth/login).
        if (usuario is null
            || !usuario.Activo
            || !passwordHasher.Verificar(usuario.PasswordHash, request.Password))
        {
            throw new UnauthorizedAppException("INVALID_CREDENTIALS", "Correo o contraseña incorrectos");
        }

        var empresa = await empresas.ObtenerAsync(usuario.EmpresaId, ct)
            ?? throw new InvalidOperationException("La empresa del usuario no existe.");

        // El servicio solo prepara el refresh token; aquí se confirma la transacción.
        var tokens = await tokenService.EmitirAsync(usuario, ct);
        await uow.SaveChangesAsync(ct);

        return Ok(new SesionResponse(
            new TokensResponse(tokens.AccessToken, tokens.RefreshToken, tokens.ExpiraEn),
            new UsuarioActual(
                usuario.UsuarioId,
                usuario.Nombre,
                usuario.Correo,
                new EmpresaResumen(empresa.EmpresaId, empresa.Nombre))));
    }
}
