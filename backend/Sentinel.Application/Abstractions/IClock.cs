namespace Sentinel.Application.Abstractions;

public interface IClock
{
    DateOnly HoyHonduras { get; }   // America/Tegucigalpa
    DateTime UtcAhora { get; }
}
