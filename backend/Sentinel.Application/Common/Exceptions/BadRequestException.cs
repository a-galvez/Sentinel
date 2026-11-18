namespace Sentinel.Application.Common.Exceptions;

public sealed class BadRequestException(string code = "BAD_REQUEST", string message = "Solicitud incorrecta.")
    : AppException(code, message);
