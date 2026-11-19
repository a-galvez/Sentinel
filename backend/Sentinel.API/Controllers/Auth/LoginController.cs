using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Sentinel.Application.Features.Auth;
using Sentinel.Application.Features.Auth.Login;

namespace Sentinel.API.Controllers.Auth;

[ApiController]
[Route("api/v1/auth")]
public sealed class LoginController(
    LoginHandler handler,
    IValidator<LoginRequest> validator) : ControllerBase
{
    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login(
        [FromBody] LoginRequest request,
        CancellationToken ct)
    {
        await validator.ValidateAndThrowAsync(request, ct);
        var response = await handler.ManejarAsync(request, ct);
        return Ok(response);
    }
}
