using Microsoft.EntityFrameworkCore;
using Sentinel.Application.Abstractions;
using Sentinel.Domain.Entities;
using Sentinel.Domain.Enums;

namespace Sentinel.Infrastructure.Persistence;

public class AppDbContext(DbContextOptions<AppDbContext> options, ICurrentTenant tenant)
    : DbContext(options)
{
    public Guid? TenantId => tenant.TieneSesion ? tenant.EmpresaId : null;

    public DbSet<Empresa> Empresas => Set<Empresa>();
    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<CategoriaProducto> CategoriasProducto => Set<CategoriaProducto>();
    public DbSet<Producto> Productos => Set<Producto>();
    public DbSet<RegistroSanitario> RegistrosSanitarios => Set<RegistroSanitario>();
    public DbSet<TipoEstablecimiento> TiposEstablecimiento => Set<TipoEstablecimiento>();
    public DbSet<Establecimiento> Establecimientos => Set<Establecimiento>();
    public DbSet<LicenciaSanitaria> Licencias => Set<LicenciaSanitaria>();
    public DbSet<TipoTramite> TiposTramite => Set<TipoTramite>();
    public DbSet<TipoDocumento> TiposDocumento => Set<TipoDocumento>();
    public DbSet<Requisito> Requisitos => Set<Requisito>();
    public DbSet<DocumentoRequisito> DocumentosRequisito => Set<DocumentoRequisito>();
    public DbSet<DocumentoDependencia> DocumentosDependencia => Set<DocumentoDependencia>();
    public DbSet<ReglaAplicabilidad> ReglasAplicabilidad => Set<ReglaAplicabilidad>();
    public DbSet<ChecklistRequisito> ChecklistRequisitos => Set<ChecklistRequisito>();
    public DbSet<AmbitoDocumento> AmbitosDocumento => Set<AmbitoDocumento>();
    public DbSet<TipoRequisito> TiposRequisito => Set<TipoRequisito>();

    protected override void OnModelCreating(ModelBuilder mb)
    {
        // Si un módulo agrega configuraciones IEntityTypeConfiguration, reemplaza este
        // bloque por: mb.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

        // Índices únicos del contrato (H1/H2/H3).
        mb.Entity<Usuario>().HasIndex(u => u.Correo).IsUnique();
        mb.Entity<RefreshToken>().HasIndex(r => r.TokenHash);

        mb.Entity<RegistroSanitario>()
            .HasIndex(r => new { r.EmpresaId, r.NumeroRegistro })
            .IsUnique();

        // Claves compuestas de los catálogos que no tienen id propio.
        mb.Entity<DocumentoRequisito>()
            .HasKey(dr => new { dr.TipoDocumentoId, dr.TipoTramiteId, dr.RequisitoId });
        mb.Entity<DocumentoDependencia>()
            .HasKey(dd => new { dd.TipoDocumentoId, dd.RequiereTipoDocumentoId });

        // Relaciones mínimas que necesitan las consultas de P3/P4/P5.
        mb.Entity<LicenciaSanitaria>()
            .HasOne(l => l.Establecimiento)
            .WithMany()
            .HasForeignKey(l => l.EstablecimientoId)
            .OnDelete(DeleteBehavior.Restrict);


        mb.Entity<LicenciaSanitaria>()
            .HasOne<TipoDocumento>()
            .WithMany()
            .HasForeignKey(l => l.TipoDocumentoId)
            .OnDelete(DeleteBehavior.Restrict);

        mb.Entity<RegistroSanitario>()
            .HasOne(r => r.Producto)
            .WithOne()
            .HasForeignKey<RegistroSanitario>(r => r.ProductoId)
            .OnDelete(DeleteBehavior.Restrict);

        mb.Entity<RegistroSanitario>()
            .HasOne<TipoDocumento>()
            .WithMany()
            .HasForeignKey(r => r.TipoDocumentoId)
            .OnDelete(DeleteBehavior.Restrict);

        mb.Entity<RegistroSanitario>()
            .Property(r => r.Estado)
            .HasDefaultValue(EstadoVencimiento.NoVerificado);


        mb.Entity<RegistroSanitario>()
            .Property(r => r.Estado)
            .HasDefaultValue(EstadoVencimiento.NoVerificado)
            .HasSentinel(EstadoVencimiento.NoVerificado);

        mb.Entity<TipoDocumento>()
            .HasOne(t => t.AmbitoDocumento)
            .WithMany()
            .HasForeignKey(t => t.AmbitoDocumentoId)
            .OnDelete(DeleteBehavior.Restrict);

        mb.Entity<Requisito>()
            .HasOne(r => r.TipoRequisito)
            .WithMany()
            .HasForeignKey(r => r.TipoRequisitoId)
            .OnDelete(DeleteBehavior.Restrict);

        mb.AplicarFiltrosGlobales(this);
    }
}
