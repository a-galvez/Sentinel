
// Sentinel.Tests/Requisitos/DbContextEnMemoria.cs
using Microsoft.EntityFrameworkCore;
using Sentinel.Infrastructure.Persistence;

namespace Sentinel.Tests.Requisitos;

public static class DbContextEnMemoria
{
    // Cada test usa una base distinta para no compartir estado.
    public static AppDbContext Crear(FakeTenant? tenant = null)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase($"sentinel-test-{Guid.NewGuid()}")
            .Options;

        return new AppDbContext(options, tenant ?? new FakeTenant());
    }
}
