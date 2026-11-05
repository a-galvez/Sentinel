using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Sentinel.Infrastructure.Data;

namespace Sentinel.Infrastructure;

public static class DependencyInjection {
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, string connectionString) {
        
        services.AddDbContext<SentinelDbContext>(options =>
            options.UseNpgsql(connectionString));

        return services;
    }
}
