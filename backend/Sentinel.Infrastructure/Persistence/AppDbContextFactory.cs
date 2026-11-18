using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Sentinel.Application.Abstractions;

namespace Sentinel.Infrastructure.Persistence;

// Usado por `dotnet ef` para generar migraciones sin levantar la API.
public sealed class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__PostgresConnection")
                               ?? "Host=localhost;Database=Sentinel;Username=postgres;Password=postgres";

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(connectionString)
            .Options;

        return new AppDbContext(options, new DesignTimeTenant());
    }

    private sealed class DesignTimeTenant : ICurrentTenant
    {
        public bool TieneSesion => false;
        public Guid EmpresaId => Guid.Empty;
        public Guid UsuarioId => Guid.Empty;
    }
}
