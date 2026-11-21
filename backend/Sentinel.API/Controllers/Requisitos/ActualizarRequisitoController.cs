using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sentinel.Application.Features.Requisitos;

namespace Sentinel.API.Controllers.Requisitos;

// PATCH /requisitos/{id} (H11 BE-8).
[ApiController]
[Authorize]
[Route("api/v1/requisitos")]
public sealed class ActualizarRequisitoController(
    ActualizarRequisitoHandler handler,
    IValidator<RequisitoUpdateRequest> validator) : ControllerBase
{
    [HttpPatch("{requisitoId:guid}")]
    public async Task<ActionResult<RequisitoDto>> Actualizar(
        Guid requisitoId,
        [FromBody] RequisitoUpdateRequest request,
        CancellationToken ct)
    {
        await validator.ValidateAndThrowAsync(request, ct);
        return Ok(await handler.ManejarAsync(requisitoId, request, ct));
    }
}
