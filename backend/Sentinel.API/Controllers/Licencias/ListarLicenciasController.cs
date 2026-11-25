using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sentinel.Application.Common;
using Sentinel.Application.Features.LicenciasSanitarias;
using Sentinel.Application.Features.LicenciasSanitarias.Listar;

namespace Sentinel.API.Controllers.Licencias;

[ApiController]
[Authorize]
[Route("api/v1/licencias")]
public sealed class ListarLicenciasController(
    ListarLicencias listar,
    IValidator<ListarLicenciasRequest> validator) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(PaginaDto<LicenciaListadoDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<PaginaDto<LicenciaListadoDto>>> Listar(
        [FromQuery] ListarLicenciasRequest request,
        CancellationToken ct)
    {
        await validator.ValidateAndThrowAsync(request, ct);
        return Ok(await listar.ManejarAsync(request, ct));
    }
}