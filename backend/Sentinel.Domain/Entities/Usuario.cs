using Sentinel.Domain.Common;

namespace Sentinel.Domain.Entities;

public class Usuario : ITenantEntity
{
    public Guid UsuarioId { get; set; }
    public Guid EmpresaId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Correo { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public bool Activo { get; set; } = true;
}
