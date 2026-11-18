using Sentinel.Domain.Common;

namespace Sentinel.Domain.Entities;

public class Establecimiento : ITenantEntity, ISoftDelete
{
    public Guid EstablecimientoId { get; set; }
    public Guid EmpresaId { get; set; }
    public int TipoEstablecimientoId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Ubicacion { get; set; } = string.Empty;
    public bool DadoDeBaja { get; set; }
}
