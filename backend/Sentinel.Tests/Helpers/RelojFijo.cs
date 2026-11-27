using Sentinel.Application.Abstractions;

namespace Sentinel.Tests.Helpers;

public class RelojFijo(DateOnly hoy) : IClock
{
    public DateOnly HoyHonduras => hoy;
    public DateTime UtcAhora => DateTime.UtcNow;
}
