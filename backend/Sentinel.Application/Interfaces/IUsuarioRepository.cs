using Sentinel.Domain.Entities;

namespace Sentinel.Application.Interfaces;

// Solo la operación base. Las demás consultas de usuario
// las agrega el encargado de autenticación.
public interface IUsuarioRepository
{
    Task<Usuario?> ObtenerPorCorreoAsync(string correoNormalizado, CancellationToken ct);
}
