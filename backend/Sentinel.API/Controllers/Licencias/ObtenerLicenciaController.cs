using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sentinel.Application.Features.LicenciasSanitarias;
using Sentinel.Application.Features.LicenciasSanitarias.Obtener;

namespace Sentinel.API.Controllers.Licencias;

[ApiController]
[Authorize]
[Route("api/v1/establecimientos/{establecimientoId:guid}/licencia")]
public sealed class ObtenerLicenciaController(ObtenerLicencia obtener) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(LicenciaSanitariaDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<LicenciaSanitariaDto>> Obtener(Guid establecimientoId, CancellationToken ct)
        => Ok(await obtener.ManejarAsync(establecimientoId, ct));
}