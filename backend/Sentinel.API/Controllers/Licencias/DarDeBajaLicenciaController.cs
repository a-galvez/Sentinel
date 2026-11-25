using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sentinel.Application.Features.LicenciasSanitarias.DarDeBaja;

namespace Sentinel.API.Controllers.Licencias;

[ApiController]
[Authorize]
[Route("api/v1/establecimientos/{establecimientoId:guid}/licencia")]
public sealed class DarDeBajaLicenciaController(DarDeBajaLicencia darDeBaja) : ControllerBase
{
    [HttpDelete]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DarDeBaja(Guid establecimientoId, CancellationToken ct)
    {
        await darDeBaja.ManejarAsync(establecimientoId, ct);
        return NoContent();
    }
}