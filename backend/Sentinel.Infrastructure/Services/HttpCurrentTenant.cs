using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Sentinel.Application.Abstractions;
using Sentinel.Application.Common.Exceptions;

namespace Sentinel.Infrastructure.Services;

public sealed class HttpCurrentTenant(IHttpContextAccessor accessor) : ICurrentTenant
{
    private ClaimsPrincipal? User => accessor.HttpContext?.User;
    public bool TieneSesion => User?.Identity?.IsAuthenticated == true;
    public Guid EmpresaId => Leer("empresaId");
    public Guid UsuarioId => Leer("sub", ClaimTypes.NameIdentifier);

    private Guid Leer(params string[] claims)
    {
        foreach (var c in claims)
            if (Guid.TryParse(User?.FindFirstValue(c), out var id)) return id;
        throw new UnauthorizedAppException("UNAUTHORIZED", "Sesión no válida.");
    }
}
