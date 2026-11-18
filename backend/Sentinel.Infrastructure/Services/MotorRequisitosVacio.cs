using Sentinel.Application.Abstractions;
using Sentinel.Domain.Entities;

namespace Sentinel.Infrastructure.Services;

// Implementación vacía de M0/M1. P5 la sustituye por la real dentro de RequisitosModule (M6).
// No toca el DbContext; sólo deja el objeto como está para que el caso de uso haga su SaveChanges.
public sealed class MotorRequisitosVacio : IMotorRequisitos
{
    public Task AplicarAProductoAsync(Producto producto, CancellationToken ct) => Task.CompletedTask;
    public Task AplicarALicenciaAsync(LicenciaSanitaria licencia, CancellationToken ct) => Task.CompletedTask;
}
