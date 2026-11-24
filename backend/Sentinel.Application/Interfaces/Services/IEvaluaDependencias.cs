namespace Sentinel.Application.Abstractions;

// Evaluación de DOCUMENTO_DEPENDENCIA al consultar (H11 BE-7). La implementa Infrastructure.
// Devuelve los documentos de los que depende `tipoDocumentoId` y que la empresa aún no tiene
// vigentes; una lista vacía significa "sin bloqueo".
public interface IEvaluaDependencias
{
    Task<IReadOnlyList<DependenciaPendiente>> SinCumplirAsync(
        int tipoDocumentoId, Guid empresaId, CancellationToken ct);
}

public sealed record DependenciaPendiente(int TipoDocumentoId, string TipoDocumentoNombre);
