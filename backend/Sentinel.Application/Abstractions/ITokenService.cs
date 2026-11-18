using Sentinel.Domain.Entities;

namespace Sentinel.Application.Abstractions;

public interface ITokenService
{
    Task<AuthTokens> EmitirAsync(Usuario usuario, CancellationToken ct);
}
