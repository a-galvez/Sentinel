using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Sentinel.Application.Features.Auth;
using Sentinel.Application.Features.Auth.Refresh;

namespace Sentinel.API.Controllers.Auth;

[ApiController]
[Route("api/v1/auth")]
public sealed class RefreshController(
    RefreshHandler handler,
    IValidator<RefreshRequest> validator) : ControllerBase
{
    [HttpPost("refresh")]
    public async Task<ActionResult<AuthResponse>> Refresh(
        [FromBody] RefreshRequest request,
        CancellationToken ct)
    {
        await validator.ValidateAndThrowAsync(request, ct);
        var response = await handler.ManejarAsync(request, ct);
        return Ok(response);
    }
}
