using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Sentinel.Domain.Common;

namespace Sentinel.Infrastructure.Persistence;

public static class TenantFilterExtensions
{
    public static void AplicarFiltrosGlobales(this ModelBuilder mb, AppDbContext ctx)
    {
        foreach (var et in mb.Model.GetEntityTypes())
        {
            if (et.IsOwned()) continue;

            var clr = et.ClrType;
            var esTenant = typeof(ITenantEntity).IsAssignableFrom(clr);
            var esSoft = typeof(ISoftDelete).IsAssignableFrom(clr);
            if (!esTenant && !esSoft) continue;

            var p = Expression.Parameter(clr, "e");
            Expression? cuerpo = null;

            if (esTenant)
            {
                // EmpresaId es Guid en el modelo; TenantId es Guid? (null = sin sesión).
                var empresa = Expression.Property(p, nameof(ITenantEntity.EmpresaId));
                var tenant = Expression.Property(Expression.Constant(ctx), nameof(AppDbContext.TenantId));
                cuerpo = Expression.Equal(
                    Expression.Convert(empresa, typeof(Guid?)),
                    tenant);
            }

            if (esSoft)
            {
                var noBaja = Expression.Not(Expression.Property(p, nameof(ISoftDelete.DadoDeBaja)));
                cuerpo = cuerpo is null ? noBaja : Expression.AndAlso(cuerpo, noBaja);
            }

            et.SetQueryFilter(Expression.Lambda(cuerpo!, p));
        }
    }
}
