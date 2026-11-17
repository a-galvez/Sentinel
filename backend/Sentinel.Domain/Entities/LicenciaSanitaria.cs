namespace Sentinel.Domain.Entities;

using Sentinel.Domain.Common;

public class LicenciaSanitaria : ITenantEntity, ISoftDelete
{
    public int LicenciaSanitariaId { get; set; }
    public int EstablecimientoId { get; set; }
    public int EmpresaId { get; set; }
    public int? TipoDocumentoId { get; set; }
    public string NumeroLicencia { get; set; } = string.Empty;
    public DateOnly FechaEmision { get; set; }
    public DateOnly FechaVencimiento { get; set; }
    public bool DadoDeBaja { get; set; }
}