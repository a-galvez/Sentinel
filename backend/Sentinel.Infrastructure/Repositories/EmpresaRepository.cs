using Microsoft.EntityFrameworkCore;
using Sentinel.Application.Abstractions.Repositories;
using Sentinel.Domain.Entities;
using Sentinel.Infrastructure.Persistence;

namespace Sentinel.Infrastructure.Repositories;

public class EmpresaRepository(AppDbContext db) : IEmpresaRepository
{
    public Task<Empresa?> ObtenerAsync(Guid empresaId, CancellationToken ct)
        => db.Empresas.SingleOrDefaultAsync(e => e.EmpresaId == empresaId, ct);
}
