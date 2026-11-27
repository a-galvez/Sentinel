namespace Sentinel.Application.Features.LicenciasSanitarias.Verificar;

public class VerificarLicenciaRequest
{
    public bool? TieneDocumento { get; set; }
    public string? Numero { get; set; }
    public DateOnly? FechaEmision { get; set; }
}