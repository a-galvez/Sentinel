using Sentinel.Domain.Entities;

namespace Sentinel.Tests.Builders;

public static class UsuarioBuilder
{
    public static Usuario Crear(
        Guid empresaId,
        string correo = "admin@demo.test",
        string nombre = "Administrador Demo",
        string passwordHash = "$2a$11$placeholder",
        bool activo = true)
        => new()
        {
            UsuarioId = Guid.NewGuid(),
            EmpresaId = empresaId,
            Nombre = nombre,
            Correo = correo,
            PasswordHash = passwordHash,
            Activo = activo
        };
}
