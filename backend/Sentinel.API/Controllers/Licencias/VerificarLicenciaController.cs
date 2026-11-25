using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sentinel.Application.Features.LicenciasSanitarias;
using Sentinel.Application.Features.LicenciasSanitarias.Verificar;

namespace Sentinel.API.Controllers.Licencias;

[ApiController]
[Authorize]
[Route("api/v1/establecimientos/{establecimientoId:guid}/licencia/verificacion")]
public sealed class VerificarLicenciaController(
    VerificarLicencia verificar,
    IValidator<VerificarLicenciaRequest> validator) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(typeof(LicenciaSanitariaDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<LicenciaSanitariaDto>> Verificar(
        Guid establecimientoId,
        [FromBody] VerificarLicenciaRequest request,
        CancellationToken ct)
    {
        await validator.ValidateAndThrowAsync(request, ct);
        return Ok(await verificar.ManejarAsync(establecimientoId, request, ct));
    }
}