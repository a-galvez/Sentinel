using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sentinel.Application.Features.LicenciasSanitarias;
using Sentinel.Application.Features.LicenciasSanitarias.RegistrarEmision;

namespace Sentinel.API.Controllers.Licencias;

[ApiController]
[Authorize]
[Route("api/v1/establecimientos/{establecimientoId:guid}/licencia")]
public sealed class RegistrarEmisionLicenciaController(
    RegistrarEmisionLicencia registrar,
    IValidator<RegistrarEmisionLicenciaRequest> validator) : ControllerBase
{
    [HttpPatch]
    [ProducesResponseType(typeof(LicenciaSanitariaDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<LicenciaSanitariaDto>> RegistrarEmision(
        Guid establecimientoId,
        [FromBody] RegistrarEmisionLicenciaRequest request,
        CancellationToken ct)
    {
        await validator.ValidateAndThrowAsync(request, ct);
        return Ok(await registrar.ManejarAsync(establecimientoId, request, ct));
    }
}