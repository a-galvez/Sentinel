using Sentinel.Domain.Entities;

namespace Sentinel.Tests.Builders;

public static class EstablecimientoBuilder
{
    public static Establecimiento Crear(
        Guid empresaId,
        int tipoEstablecimientoId = 1,
        string nombre = "Bodega Demo",
        string ubicacion = "Tegucigalpa")
        => new()
        {
            EstablecimientoId = Guid.NewGuid(),
            EmpresaId = empresaId,
            TipoEstablecimientoId = tipoEstablecimientoId,
            Nombre = nombre,
            Ubicacion = ubicacion,
            DadoDeBaja = false
        };
}
