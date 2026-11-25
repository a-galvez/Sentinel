namespace Sentinel.Application.Features.LicenciasSanitarias.RegistrarEmision;

public class RegistrarEmisionLicenciaRequest
{
    public string Numero { get; set; } = string.Empty;
    public DateOnly? FechaEmision { get; set; }
}