using Sentinel.Domain.Entities;
using Sentinel.Domain.Enums;

namespace Sentinel.Tests.Builders;

public static class RegistroSanitarioBuilder
{
    public static RegistroSanitario Crear(
        Guid empresaId,
        Guid productoId,
        Guid? registroSanitarioId = null,
        int tipoDocumentoId = 1,
        string? numeroRegistro = null,
        DateOnly? fechaEmision = null,
        DateOnly? fechaVencimiento = null,
        EstadoVencimiento estado = EstadoVencimiento.NoVerificado)
        => new()
        {
            RegistroSanitarioId = registroSanitarioId ?? Guid.NewGuid(),
            EmpresaId = empresaId,
            ProductoId = productoId,
            TipoDocumentoId = tipoDocumentoId,
            NumeroRegistro = numeroRegistro,
            FechaEmision = fechaEmision,
            FechaVencimiento = fechaVencimiento,
            Estado = estado,
            DadoDeBaja = false
        };
}
