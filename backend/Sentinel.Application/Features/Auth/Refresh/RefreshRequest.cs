using System.Text.Json.Serialization;

namespace Sentinel.Application.Features.Auth.Refresh;

// DTO de entrada del POST /auth/refresh. El contrato usa `refresh_token` (snake_case).
public sealed record RefreshRequest(
    [property: JsonPropertyName("refresh_token")] string RefreshToken);
