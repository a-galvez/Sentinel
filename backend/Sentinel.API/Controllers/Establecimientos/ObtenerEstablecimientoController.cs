using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sentinel.Application.Features.Establecimientos;
using Sentinel.Application.Features.Establecimientos.Obtener;

namespace Sentinel.API.Controllers.Establecimientos;

[ApiController]
[Authorize]
[Route("api/v1/establecimientos")]
public sealed class ObtenerEstablecimientoController(ObtenerEstablecimiento obtener) : ControllerBase
{
    [HttpGet("{establecimientoId:guid}")]
    [ProducesResponseType(typeof(EstablecimientoDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<EstablecimientoDto>> Obtener(Guid establecimientoId, CancellationToken ct)
        => Ok(await obtener.ManejarAsync(establecimientoId, ct));
}