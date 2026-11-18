namespace Sentinel.Application.Common.Exceptions;

public sealed class UnauthorizedAppException(string code = "UNAUTHORIZED", string message = "Sesión no válida.")
    : AppException(code, message);
