using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sentinel.Application.Common;
using Sentinel.Application.Features.Establecimientos;
using Sentinel.Application.Features.Establecimientos.Listar;

namespace Sentinel.API.Controllers.Establecimientos;

[ApiController]
[Authorize]
[Route("api/v1/establecimientos")]
public sealed class ListarEstablecimientosController(
    ListarEstablecimientos listar,
    IValidator<ListarEstablecimientosRequest> validator) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(PaginaDto<EstablecimientoDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<PaginaDto<EstablecimientoDto>>> Listar(
        [FromQuery] ListarEstablecimientosRequest request,
        CancellationToken ct)
    {
        await validator.ValidateAndThrowAsync(request, ct);
        return Ok(await listar.ManejarAsync(request, ct));
    }
}