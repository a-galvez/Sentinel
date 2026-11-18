using Sentinel.Domain.Entities;

namespace Sentinel.Tests.Builders;

public static class LicenciaBuilder
{
    private static readonly DateOnly Hoy = DateOnly.FromDateTime(DateTime.UtcNow);

    public static LicenciaSanitaria Crear(
        Guid empresaId,
        Guid establecimientoId,
        string? numeroLicencia = null,
        DateOnly? fechaEmision = null,
        DateOnly? fechaVencimiento = null)
        => new()
        {
            LicenciaSanitariaId = Guid.NewGuid(),
            EmpresaId = empresaId,
            EstablecimientoId = establecimientoId,
            NumeroLicencia = numeroLicencia ?? $"LIC-{Guid.NewGuid():N}",
            FechaEmision = fechaEmision ?? Hoy,
            FechaVencimiento = fechaVencimiento ?? Hoy.AddYears(1),
            DadoDeBaja = false
        };
}
