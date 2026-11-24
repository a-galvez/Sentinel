using Sentinel.Domain.Entities;

namespace Sentinel.Application.Interfaces;

// Solo la operación base. El resto de lecturas/existencias/escrituras
// las agrega el encargado del catálogo.
public interface ICatalogoRepository
{
    Task<List<CategoriaProducto>> CategoriasProductoAsync(CancellationToken ct);
}
