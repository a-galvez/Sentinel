using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sentinel.Application.Common;
using Sentinel.Application.Features.LicenciasSanitarias;
using Sentinel.Application.Features.LicenciasSanitarias.DarDeBaja;
using Sentinel.Application.Features.LicenciasSanitarias.Listar;
using Sentinel.Application.Features.LicenciasSanitarias.Obtener;
using Sentinel.Application.Features.LicenciasSanitarias.RegistrarEmision;
using Sentinel.Application.Features.LicenciasSanitarias.Verificar;

namespace Sentinel.API.Controllers.Licencias;

[ApiController]
[Authorize]
[Route("api/v1")]
public sealed class LicenciasController(
    ListarLicencias listar,
    IValidator<ListarLicenciasRequest> listarValidator,
    ObtenerLicencia obtener,
    RegistrarEmisionLicencia registrarEmision,
    IValidator<RegistrarEmisionLicenciaRequest> emisionValidator,
    DarDeBajaLicencia darDeBaja,
    VerificarLicencia verificar) : ControllerBase
{
    [HttpGet("licencias")]
    [ProducesResponseType(typeof(PaginaDto<LicenciaListadoDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<PaginaDto<LicenciaListadoDto>>> Listar(
        [FromQuery] ListarLicenciasRequest request, CancellationToken ct)
    {
        await listarValidator.ValidateAndThrowAsync(request, ct);
        return Ok(await listar.ManejarAsync(request, ct));
    }

    [HttpGet("establecimientos/{establecimientoId:guid}/licencia")]
    [ProducesResponseType(typeof(LicenciaSanitariaDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<LicenciaSanitariaDto>> Obtener(
        Guid establecimientoId, CancellationToken ct)
        => Ok(await obtener.ManejarAsync(establecimientoId, ct));

    [HttpPatch("establecimientos/{establecimientoId:guid}/licencia")]
    [ProducesResponseType(typeof(LicenciaSanitariaDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<LicenciaSanitariaDto>> RegistrarEmision(
        Guid establecimientoId,
        [FromBody] RegistrarEmisionLicenciaRequest request,
        CancellationToken ct)
    {
        await emisionValidator.ValidateAndThrowAsync(request, ct);
        return Ok(await registrarEmision.ManejarAsync(establecimientoId, request, ct));
    }

    [HttpDelete("establecimientos/{establecimientoId:guid}/licencia")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DarDeBaja(Guid establecimientoId, CancellationToken ct)
    {
        await darDeBaja.ManejarAsync(establecimientoId, ct);
        return NoContent();
    }

    [HttpPost("establecimientos/{establecimientoId:guid}/licencia/verificacion")]
    [ProducesResponseType(typeof(LicenciaSanitariaDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<LicenciaSanitariaDto>> Verificar(
        Guid establecimientoId,
        [FromBody] VerificarLicenciaRequest request,
        CancellationToken ct)
        => Ok(await verificar.ManejarAsync(establecimientoId, request, ct));
}
