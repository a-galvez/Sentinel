using Microsoft.EntityFrameworkCore;
using Sentinel.Domain.Entities;

namespace Sentinel.Application.Abstractions;

public interface IAppDbContext
{
    DbSet<Empresa> Empresas { get; }
    DbSet<Usuario> Usuarios { get; }
    DbSet<RefreshToken> RefreshTokens { get; }
    DbSet<CategoriaProducto> CategoriasProducto { get; }
    DbSet<Producto> Productos { get; }
    DbSet<TipoEstablecimiento> TiposEstablecimiento { get; }
    DbSet<Establecimiento> Establecimientos { get; }
    DbSet<LicenciaSanitaria> Licencias { get; }
    DbSet<TipoTramite> TiposTramite { get; }
    DbSet<TipoDocumento> TiposDocumento { get; }
    DbSet<Requisito> Requisitos { get; }
    DbSet<DocumentoRequisito> DocumentosRequisito { get; }
    DbSet<DocumentoDependencia> DocumentosDependencia { get; }
    DbSet<ReglaAplicabilidad> ReglasAplicabilidad { get; }
    DbSet<ChecklistRequisito> ChecklistRequisitos { get; }
    DbSet<AmbitoDocumento> AmbitosDocumento { get; }
    DbSet<TipoRequisito> TiposRequisito { get; }

    Task<int> SaveChangesAsync(CancellationToken ct);
}
