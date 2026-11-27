namespace Sentinel.Application.Common.Exceptions;

public abstract class AppException(string code, string message) : Exception(message)
{
    public string Code { get; } = code;

    // Errores por campo para el formato RFC 7807 (`errors`).
    // Es opcional: la mayoría de excepciones solo necesitan code/message.
    public IReadOnlyDictionary<string, string[]>? Errors { get; init; }
}
