using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sentinel.Application.Features.Establecimientos;
using Sentinel.Application.Features.Establecimientos.Crear;

namespace Sentinel.API.Controllers.Establecimientos;

[ApiController]
[Authorize]
[Route("api/v1/establecimientos")]
public sealed class CrearEstablecimientoController(
    CrearEstablecimiento crear,
    IValidator<CrearEstablecimientoRequest> validator) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(typeof(EstablecimientoDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<EstablecimientoDto>> Crear(
        [FromBody] CrearEstablecimientoRequest request, CancellationToken ct)
    {
        await validator.ValidateAndThrowAsync(request, ct);
        var dto = await crear.ManejarAsync(request, ct);
        return Created($"api/v1/establecimientos/{dto.EstablecimientoId}", dto);
    }
}