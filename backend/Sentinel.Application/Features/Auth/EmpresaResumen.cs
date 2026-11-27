namespace Sentinel.Application.Features.Auth;

// Empresa de la sesión según el contrato UsuarioActual.empresa.
public class EmpresaResumen
{
    public EmpresaResumen(Guid empresaId, string nombre)
    {
        EmpresaId = empresaId;
        Nombre = nombre;
    }

    public Guid EmpresaId { get; }
    public string Nombre { get; }
}
