namespace Sentinel.Application.Features.Auth.Login;

// DTO de entrada del POST /auth/login. Sin empresaId: sale del token, no del cuerpo.
public sealed record LoginRequest(string Correo, string Contrasena);
