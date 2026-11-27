using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sentinel.Application.Abstractions;
using Sentinel.Application.Abstractions.Repositories;
using Sentinel.Application.Common.Exceptions;
using Sentinel.Application.Features.Auth;

namespace Sentinel.API.Controllers.Auth;

// GET /auth/me (H2). Usuario y empresa de la sesión actual.
[ApiController]
[Authorize]
[Route("api/v1/auth")]
public sealed class MeController(
    ICurrentTenant tenant,
    IUsuarioRepository usuarios,
    IEmpresaRepository empresas) : ControllerBase
{
    [HttpGet("me")]
    public async Task<ActionResult<UsuarioActual>> Me(CancellationToken ct)
    {
        var usuario = await usuarios.ObtenerPorIdAsync(tenant.UsuarioId, ct);

        // Token válido pero usuario eliminado o inactivo: la sesión ya no sirve.
        if (usuario is null || !usuario.Activo)
            throw new UnauthorizedAppException("UNAUTHORIZED", "Sesión no válida.");

        var empresa = await empresas.ObtenerAsync(usuario.EmpresaId, ct)
            ?? throw new NotFoundException("Empresa no encontrada.");

        return Ok(new UsuarioActual(
            usuario.UsuarioId,
            usuario.Nombre,
            usuario.Correo,
            new EmpresaResumen(empresa.EmpresaId, empresa.Nombre)));
    }
}
