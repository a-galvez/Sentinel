
// Sentinel.Tests/Requisitos/FakeTenant.cs
using Sentinel.Application.Abstractions;

namespace Sentinel.Tests.Requisitos;

// Tenant sin sesión: TenantId = null, así el filtro global no oculta nada del seed
// (los catálogos no son tenant-filtered, pero los tests del motor sí necesitan control).
public sealed class FakeTenant(Guid? empresaId = null, Guid? usuarioId = null) : ICurrentTenant
{
    public bool TieneSesion => empresaId.HasValue;
    public Guid EmpresaId => empresaId ?? Guid.Empty;
    public Guid UsuarioId => usuarioId ?? Guid.Empty;
}
