using System.Text.Json.Serialization;
using Sentinel.Domain.Enums;

namespace Sentinel.Application.Features.Requisitos;

// PATCH /requisitos/{id}. Semántica parcial: cada campo no nulo se aplica.
// `responsableId` sigue la HU H11 BE-8 (usuario de la empresa), no el texto libre del YAML.
// Para quitar el responsable se envía Guid.Empty (el contrato no tiene un "null explícito").
public sealed record RequisitoUpdateRequest(
    [property: JsonPropertyName("estado")] EstadoRequisito? Estado,
    [property: JsonPropertyName("responsableId")] Guid? ResponsableId,
    [property: JsonPropertyName("fechaLimite")] DateOnly? FechaLimite);
