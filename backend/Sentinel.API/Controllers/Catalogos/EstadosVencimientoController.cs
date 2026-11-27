using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sentinel.Application.Features.Catalogos;

namespace Sentinel.API.Controllers.Catalogos;

// GET /catalogos/estados-vencimiento.
// No usa repositorio: los estados salen del enum de Domain.
[ApiController]
[Authorize]
[Route("api/v1/catalogos")]
public sealed class EstadosVencimientoController : ControllerBase
{
    [HttpGet("estados-vencimiento")]
    public ActionResult<IReadOnlyList<EstadoCatalogoItemDto>> Listar()
        => Ok(EstadoVencimientoCatalogo.Listar());
}
