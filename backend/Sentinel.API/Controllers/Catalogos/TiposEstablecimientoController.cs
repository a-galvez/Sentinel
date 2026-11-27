using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sentinel.Application.Abstractions.Repositories;
using Sentinel.Application.Features.Catalogos;

namespace Sentinel.API.Controllers.Catalogos;

// GET /catalogos/tipos-establecimiento.
[ApiController]
[Authorize]
[Route("api/v1/catalogos")]
public sealed class TiposEstablecimientoController(ICatalogoRepository catalogos) : ControllerBase
{
    [HttpGet("tipos-establecimiento")]
    public async Task<ActionResult<IReadOnlyList<CatalogoItemDto>>> Listar(CancellationToken ct)
    {
        var items = await catalogos.TiposEstablecimientoAsync(ct);

        return Ok(items
            .Select(t => new CatalogoItemDto
            {
                Id = t.TipoEstablecimientoId,
                Nombre = t.Nombre
            })
            .ToList());
    }
}
