
using Sentinel.Domain.Enums;

namespace Sentinel.Application.Features.Catalogos;

// Orden de presentación del contrato. No usamos Enum.GetValues porque el orden
// numérico del enum está condicionado por la persistencia de RegistroSanitario.Estado.
public static class EstadoVencimientoCatalogo
{
    public static IReadOnlyList<EstadoCatalogoItemDto> Listar() =>
    [
        Crear(EstadoVencimiento.NoVerificado),
        Crear(EstadoVencimiento.EnTramite),
        Crear(EstadoVencimiento.Vigente),
        Crear(EstadoVencimiento.PorVencer),
        Crear(EstadoVencimiento.Vencido)
    ];

    // `Codigo` es el nombre del enum: el enum serializa como número (no hay
    // JsonStringEnumConverter), y el contrato expone el nombre.
    private static EstadoCatalogoItemDto Crear(EstadoVencimiento estado) => new()
    {
        Codigo = estado.ToString(),
        Nombre = NombreDe(estado)
    };

    private static string NombreDe(EstadoVencimiento estado) => estado switch
    {
        EstadoVencimiento.NoVerificado => "No verificado",
        EstadoVencimiento.EnTramite => "En trámite",
        EstadoVencimiento.Vigente => "Vigente",
        EstadoVencimiento.PorVencer => "Por vencer",
        EstadoVencimiento.Vencido => "Vencido",
        _ => throw new ArgumentOutOfRangeException(nameof(estado), estado, "Estado sin etiqueta de catálogo.")
    };
}
