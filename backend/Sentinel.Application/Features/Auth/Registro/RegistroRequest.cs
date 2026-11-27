namespace Sentinel.Application.Features.Auth.Registro;

// DTO de entrada del POST /auth/registro (H1).
// Empresa y administrador se crean juntos en una sola transacción.
public class RegistroRequest
{
    public RegistroRequest()
    {
    }

    public RegistroRequest(string nombreEmpresa, string nombre, string correo, string password)
    {
        NombreEmpresa = nombreEmpresa;
        Nombre = nombre;
        Correo = correo;
        Password = password;
    }

    public string NombreEmpresa { get; init; } = string.Empty;
    public string Nombre { get; init; } = string.Empty;
    public string Correo { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
}
