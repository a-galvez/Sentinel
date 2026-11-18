using Sentinel.Domain.Entities;

namespace Sentinel.Tests.Builders;

public static class EmpresaBuilder
{
    public static Empresa Crear(string nombre = "Farmacia Demo")
        => new()
        {
            EmpresaId = Guid.NewGuid(),
            Nombre = nombre,
            FechaRegistro = DateTime.UtcNow
        };
}
