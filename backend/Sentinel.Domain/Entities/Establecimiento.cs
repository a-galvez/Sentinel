namespace Sentinel.Domain.Entities;

using Sentinel.Domain.Common;

public class Establecimiento : ITenantEntity, ISoftDelete
{
    public int EstablecimientoId { get; set; }
    public int EmpresaId { get; set; }
    public int TipoEstablecimientoId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Ubicacion { get; set; } = string.Empty;
    public bool DadoDeBaja { get; set; }
}