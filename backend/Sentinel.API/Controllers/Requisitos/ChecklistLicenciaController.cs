using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sentinel.Application.Features.Requisitos;

namespace Sentinel.API.Controllers.Requisitos;

// GET /licencias/{id}/checklist. Una clase por caso de uso, ruta base api/v1/licencias.
[ApiController]
[Authorize]
[Route("api/v1/licencias")]
public sealed class ChecklistLicenciaController(ChecklistReader reader) : ControllerBase
{
    [HttpGet("{licenciaId:guid}/checklist")]
    public async Task<ActionResult<ChecklistDto>> Obtener(Guid licenciaId, CancellationToken ct)
        => Ok(await reader.ParaLicenciaAsync(licenciaId, ct));
}
