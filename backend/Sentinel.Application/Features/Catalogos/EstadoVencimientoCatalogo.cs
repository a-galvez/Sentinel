namespace Sentinel.Application.Features.Catalogos;

// Orden de presentación del contrato. No usamos Enum.GetValues porque el orden
// numérico del enum está condicionado por la persistencia de RegistroSanitario.Estado.
public static class EstadoVencimientoCatalogo
{
    public static IReadOnlyList<EstadoCatalogoItemDto> Listar() =>
    [
        new() { Codigo = "NoVerificado", Nombre = "No verificado" },
        new() { Codigo = "EnTramite", Nombre = "En trámite" },
        new() { Codigo = "Vigente", Nombre = "Vigente" },
        new() { Codigo = "PorVencer", Nombre = "Por vencer" },
        new() { Codigo = "Vencido", Nombre = "Vencido" }
    ];
}
