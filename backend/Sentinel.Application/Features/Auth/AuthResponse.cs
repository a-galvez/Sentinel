namespace Sentinel.Application.Features.Auth;

// Responde el contrato AuthResponse: access_token, refresh_token, token_type y user.
// Lo usan tanto login como refresh.
public sealed record AuthResponse(
    string AccessToken,
    string RefreshToken,
    string TokenType,
    UsuarioDto User);
