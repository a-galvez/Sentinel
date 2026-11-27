namespace Sentinel.Application.Features.Auth;

// Usuario autenticado según el contrato UsuarioActual (login, refresh y /auth/me).
public class UsuarioActual
{
    public UsuarioActual(Guid usuarioId, string nombre, string correo, EmpresaResumen empresa)
    {
        UsuarioId = usuarioId;
        Nombre = nombre;
        Correo = correo;
        Empresa = empresa;
    }

    public Guid UsuarioId { get; }
    public string Nombre { get; }
    public string Correo { get; }
    public EmpresaResumen Empresa { get; }
}
