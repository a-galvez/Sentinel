using Sentinel.Domain.Entities;

namespace Sentinel.Application.Abstractions;

// El motor solo responde, que documento le toca?. No escribe entidades ni toca checklist
// Eso lo hacen los casos de uso
public interface IMotorRequisitos
{
    //Devuelve el TipoDocumentoId que aplica al producto\
    // o null si ninguna regla aplica
    Task<int?> ElegirDocumentoParaProductoAsync(Producto producto, CancellationToken ct);

    //Devuele el TipoDocumentoId que aplica al establecimiento
    // o null si ninguna regla aplica
    Task<int?> ElegirDocumentoParaEstablecimientoAsync(Establecimiento establecimiento, CancellationToken ct);
}
