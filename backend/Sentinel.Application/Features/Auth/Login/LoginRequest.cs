namespace Sentinel.Application.Features.Auth.Login;

// DTO de entrada del POST /auth/login. Sin empresaId: sale del token, no del cuerpo.
// El contrato usa `password`, por eso la propiedad se llama así.
public class LoginRequest
{
    public LoginRequest()
    {
    }

    public LoginRequest(string correo, string password)
    {
        Correo = correo;
        Password = password;
    }

    public string Correo { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
}
