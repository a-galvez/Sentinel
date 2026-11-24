namespace Sentinel.Application.Abstractions;

public interface ICurrentTenant
{
    bool TieneSesion { get; }
    Guid EmpresaId { get; }   // lanza UnauthorizedAppException si no hay sesión
    Guid UsuarioId { get; }
}
