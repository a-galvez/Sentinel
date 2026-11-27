using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sentinel.Application.Abstractions.Repositories;
using Sentinel.Application.Features.Catalogos;

namespace Sentinel.API.Controllers.Catalogos;

// GET /catalogos/categorias-producto (H? catálogos globales).
[ApiController]
[Authorize]
[Route("api/v1/catalogos")]
public sealed class CategoriasProductoController(ICatalogoRepository catalogos) : ControllerBase
{
    [HttpGet("categorias-producto")]
    public async Task<ActionResult<IReadOnlyList<CatalogoItemDto>>> Listar(CancellationToken ct)
    {
        var items = await catalogos.CategoriasProductoAsync(ct);

        return Ok(items
            .Select(c => new CatalogoItemDto
            {
                Id = c.CategoriaProductoId,
                Nombre = c.Nombre
            })
            .ToList());
    }
}
