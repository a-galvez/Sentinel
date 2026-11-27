using Sentinel.Domain.Entities;

namespace Sentinel.Application.Abstractions.Repositories;

public interface IRegistroSanitarioRepository
{
    //Busca un registro por su ID único
    Task<RegistroSanitario?> ObtenerAsync(Guid registroSanitarioId, CancellationToken ct);
    //Verifica si un producto ya tiene un registro sanitario asociado
    Task<RegistroSanitario?> ObtenerPorProductoAsync(Guid productoId, CancellationToken ct);

    //Revisa si ya existe un número de registro para una misma empresa
    Task<bool> ExisteNumeroAsync(
        Guid empresaId,
        string numero,
        Guid? excluirRegistroSanitarioId,
        CancellationToken ct
    );

    //Marca una nueva entidad para se guardada
    void Agregar(RegistroSanitario registro);
}
