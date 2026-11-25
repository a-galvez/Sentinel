using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sentinel.Application.Features.Establecimientos.DarDeBaja;

namespace Sentinel.API.Controllers.Establecimientos;

[ApiController]
[Authorize]
[Route("api/v1/establecimientos")]
public sealed class DarDeBajaEstablecimientoController(DarDeBajaEstablecimiento darDeBaja) : ControllerBase
{
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