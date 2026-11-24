using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sentinel.Application.Features.Requisitos;

namespace Sentinel.API.Controllers.Requisitos;

// GET /productos/{id}/checklist (H11 BE-6).
[ApiController]
[Authorize]
[Route("api/v1/productos")]
public sealed class ChecklistProductoController(ChecklistReader reader) : ControllerBase
{
    [HttpGet("{productoId:guid}/checklist")]
    public async Task<ActionResult<ChecklistDto>> Obtener(Guid productoId, CancellationToken ct)
        => Ok(await reader.ParaProductoAsync(productoId, ct));
}
