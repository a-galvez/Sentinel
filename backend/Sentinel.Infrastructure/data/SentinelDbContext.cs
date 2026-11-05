using Microsoft.EntityFrameworkCore;

namespace Sentinel.Infrastructure.Data;

public class SentinelDbContext : DbContext {
    public SentinelDbContext(DbContextOptions<SentinelDbContext> options) : base(options) { }
    
}
