using Sentinel.Domain.Enums;

namespace Sentinel.Application.Features.Requisitos;

// DTOs del contrato (contratos-api-sentinel.yaml: Checklist, Requisito).
// `responsableId` sigue la HU H11 BE-8 (usuario de la empresa); el YAML todavía dice
// `responsable` como texto libre: P1 debe alinearlo (aviso enviado).

public sealed record RequisitoDto(
    Guid Id,
    int Orden,
    string Descripcion,
    TipoRequisito Tipo,
    EstadoRequisito Estado,
    Guid? ResponsableId,
    string? ResponsableNombre,
    DateOnly? FechaLimite,
    Guid? CumplidoPorId,
    string? CumplidoPorNombre,
    DateTime? CumplidoEn);

public sealed record AvanceDto(int Cumplidos, int Total);

public sealed record TipoDocumentoDto(int Id, string Codigo, string Nombre);

public sealed record TramiteDto(int Id, string Nombre);

// Un documento del que depende el consultado y que aún no está vigente.
public sealed record BloqueoPorDependenciaDto(int TipoDocumentoId, string TipoDocumentoNombre);

public sealed record ChecklistDto(
    TramiteDto? Tramite,
    TipoDocumentoDto? TipoDocumento,
    IReadOnlyList<RequisitoDto> Requisitos,
    AvanceDto Avance,
    IReadOnlyList<BloqueoPorDependenciaDto> Bloqueo);
