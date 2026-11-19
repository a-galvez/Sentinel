using Sentinel.Domain.Entities;

namespace Sentinel.Application.Features.Auth;

// DTO de usuario para el contrato H2 (login y /users/me). `Rol` es post-MVP (H13):
// en el MVP todos los usuarios tienen los mismos permisos, así que va en null.
public sealed record UsuarioDto(
    Guid Id,
    Guid EmpresaId,
    string Nombre,
    string Correo,
    bool Activo,
    string? Rol)
{
    public static UsuarioDto Desde(Usuario u) =>
        new(u.UsuarioId, u.EmpresaId, u.Nombre, u.Correo, u.Activo, null);
}
