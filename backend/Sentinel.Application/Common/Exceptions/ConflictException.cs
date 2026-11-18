namespace Sentinel.Application.Common.Exceptions;

public sealed class ConflictException(string code = "CONFLICT", string message = "El recurso ya existe.")
    : AppException(code, message);
