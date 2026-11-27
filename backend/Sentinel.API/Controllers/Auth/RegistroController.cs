using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Sentinel.Application.Abstractions;
using Sentinel.Application.Abstractions.Repositories;
using Sentinel.Application.Common;
using Sentinel.Application.Common.Exceptions;
using Sentinel.Application.Features.Auth;
using Sentinel.Application.Features.Auth.Registro;
using Sentinel.Domain.Entities;

namespace Sentinel.API.Controllers.Auth;

// POST /auth/registro (H1). Público.
// Crea Empresa + Usuario administrador + refresh token en una sola transacción.
[ApiController]
[Route("api/v1/auth")]
public sealed class RegistroController(
    IUsuarioRepository usuarios,
    IEmpresaRepository empresas,
    IPasswordHasher passwordHasher,
    ITokenService tokenService,
    IUnitOfWork uow,
    IClock clock,
    IValidator<RegistroRequest> validator) : ControllerBase
{
    [HttpPost("registro")]
    public async Task<ActionResult<SesionResponse>> Registro(
        [FromBody] RegistroRequest request,
        CancellationToken ct)
    {
        await validator.ValidateAndThrowAsync(request, ct);

        var correo = Correo.Normalizar(request.Correo);

        if (await usuarios.ExistePorCorreoAsync(correo, ct))
        {
            throw new ConflictException(
                "CORREO_YA_REGISTRADO",
                "Ya existe una cuenta con ese correo.")
            {
                Errors = new Dictionary<string, string[]>
                {
                    ["correo"] = ["El correo ya está registrado."]
                }
            };
        }

        var empresa = new Empresa
        {
            EmpresaId = Guid.NewGuid(),
            Nombre = request.NombreEmpresa.Trim(),
            FechaRegistro = clock.UtcAhora
        };

        var usuario = new Usuario
        {
            UsuarioId = Guid.NewGuid(),
            EmpresaId = empresa.EmpresaId,
            Nombre = request.Nombre.Trim(),
            Correo = correo,
            PasswordHash = passwordHasher.Hash(request.Password),
            Activo = true
        };

        empresas.Agregar(empresa);
        usuarios.Agregar(usuario);

        // El servicio solo deja el refresh token preparado; aquí se confirma todo junto.
        var tokens = await tokenService.EmitirAsync(usuario, ct);
        await uow.SaveChangesAsync(ct);

        var response = new SesionResponse(
            new TokensResponse(tokens.AccessToken, tokens.RefreshToken, tokens.ExpiraEn),
            new UsuarioActual(
                usuario.UsuarioId,
                usuario.Nombre,
                usuario.Correo,
                new EmpresaResumen(empresa.EmpresaId, empresa.Nombre)));

        return StatusCode(StatusCodes.Status201Created, response);
    }
}
