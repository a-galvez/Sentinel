using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Sentinel.Infrastructure.Persistence;

namespace Sentinel.Infrastructure;

// Encapsula la migración al arranque para que la capa API no conozca AppDbContext.
public static class DatabaseMigrationExtensions
{
    public static async Task MigrarBaseDeDatosAsync(
        this IServiceProvider services,
        CancellationToken ct = default)
    {
        await using var scope = services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await db.Database.MigrateAsync(ct);
    }
}
