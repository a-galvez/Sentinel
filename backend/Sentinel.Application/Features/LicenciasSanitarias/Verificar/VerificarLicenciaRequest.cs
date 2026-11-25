namespace Sentinel.Application.Features.LicenciasSanitarias.Verificar;

public class VerificarLicenciaRequest
{
    public bool? TieneLicencia { get; set; }
    public string? Numero { get; set; }
    public DateOnly? FechaEmision { get; set; }
}