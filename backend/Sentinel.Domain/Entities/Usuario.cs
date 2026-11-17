namespace Sentinel.Domain.Entities;

using Sentinel.Domain.Common;

public class Usuario : ITenantEntity
{
    public int UsuarioId { get; set; }
    public int EmpresaId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Correo { get; set; } = string.Empty; 
    public string PasswordHash { get; set; } = string.Empty;
    public bool Activo { get; set; } = true;
}