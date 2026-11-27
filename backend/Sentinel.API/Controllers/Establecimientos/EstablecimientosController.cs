using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sentinel.Application.Common;
using Sentinel.Application.Features.Establecimientos;
using Sentinel.Application.Features.Establecimientos.Crear;
using Sentinel.Application.Features.Establecimientos.DarDeBaja;
using Sentinel.Application.Features.Establecimientos.Listar;
using Sentinel.Application.Features.Establecimientos.Obtener;

namespace Sentinel.API.Controllers.Establecimientos;

[ApiController]
[Authorize]
[Route("api/v1/establecimientos")]
public sealed class EstablecimientosController(
    CrearEstablecimiento crear,
    IValidator<CrearEstablecimientoRequest> crearValidator,
    ListarEstablecimientos listar,
    IValidator<ListarEstablecimientosRequest> listarValidator,
    ObtenerEstablecimiento obtener,
    DarDeBajaEstablecimiento darDeBaja) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(typeof(EstablecimientoCreadoDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<EstablecimientoCreadoDto>> Crear(
        [FromBody] CrearEstablecimientoRequest request, CancellationToken ct)
    {
        await crearValidator.ValidateAndThrowAsync(request, ct);
        var dto = await crear.ManejarAsync(request, ct);
        return Created($"/api/v1/establecimientos/{dto.Establecimiento.EstablecimientoId}", dto);
    }

    [HttpGet]
    [ProducesResponseType(typeof(PaginaDto<EstablecimientoDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<PaginaDto<EstablecimientoDto>>> Listar(
        [FromQuery] ListarEstablecimientosRequest request, CancellationToken ct)
    {
        await listarValidator.ValidateAndThrowAsync(request, ct);
        return Ok(await listar.ManejarAsync(request, ct));
    }

    [HttpGet("{establecimientoId:guid}")]
    [ProducesResponseType(typeof(EstablecimientoDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<EstablecimientoDto>> Obtener(
        Guid establecimientoId, CancellationToken ct)
        => Ok(await obtener.ManejarAsync(establecimientoId, ct));

    [HttpDelete("{establecimientoId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DarDeBaja(Guid establecimientoId, CancellationToken ct)
    {
        await darDeBaja.ManejarAsync(establecimientoId, ct);
        return NoContent();
    }
}
