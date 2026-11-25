using Sentinel.Domain.Common;
using Sentinel.Domain.Enums;

namespace Sentinel.Domain.Entities;

public class LicenciaSanitaria : ITenantEntity, ISoftDelete
{
    public Guid LicenciaSanitariaId { get; set; }
    public Guid EstablecimientoId { get; set; }
    public Guid EmpresaId { get; set; }
    public int? TipoDocumentoId { get; set; }
    public EstadoVencimiento Estado { get; set; } = EstadoVencimiento.NoVerificado;
    public string? NumeroLicencia { get; set; }          // null hasta emitirse
    public DateOnly? FechaEmision { get; set; }          // null hasta emitirse
    public DateOnly? FechaVencimiento { get; set; }      // null hasta emitirse
    public bool DadoDeBaja { get; set; }

    public Establecimiento? Establecimiento { get; set; }
}