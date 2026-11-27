using Sentinel.Domain.Entities;

namespace Sentinel.Application.Features.TiposDocumento;

public static class TipoDocumentoDtoExtensions
{
    public static TipoDocumentoResumenDto ToResumenDto(this TipoDocumento tipo) => new()
    {
        TipoDocumentoId = tipo.TipoDocumentoId,
        Codigo = tipo.Codigo,
        Nombre = tipo.Nombre,
        AutoridadEmisora = tipo.AutoridadEmisora,
        BaseLegal = tipo.BaseLegal,
        VigenciaMeses = tipo.VigenciaMeses
    };
}
