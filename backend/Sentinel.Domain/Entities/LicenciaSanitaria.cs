using Sentinel.Domain.Common;

namespace Sentinel.Domain.Entities;

public class LicenciaSanitaria : ITenantEntity, ISoftDelete
{
    public Guid LicenciaSanitariaId { get; set; }
    public Guid EstablecimientoId { get; set; }
    public Guid EmpresaId { get; set; }
    public int? TipoDocumentoId { get; set; }
    public string NumeroLicencia { get; set; } = string.Empty;
    public DateOnly FechaEmision { get; set; }
    public DateOnly FechaVencimiento { get; set; }
    public bool DadoDeBaja { get; set; }

    public Establecimiento? Establecimiento { get; set; }
}
