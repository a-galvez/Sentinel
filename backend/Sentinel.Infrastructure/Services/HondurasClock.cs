using Sentinel.Application.Abstractions;

namespace Sentinel.Infrastructure.Services;

public sealed class HondurasClock : IClock
{
    public DateTime UtcAhora => DateTime.UtcNow;
    // Honduras es UTC-6 todo el año; el desfase fijo evita depender de tzdata en Docker.
    public DateOnly HoyHonduras => DateOnly.FromDateTime(UtcAhora.AddHours(-6));
}
