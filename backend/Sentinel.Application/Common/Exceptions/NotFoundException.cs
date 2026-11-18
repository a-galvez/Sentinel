namespace Sentinel.Application.Common.Exceptions;

public sealed class NotFoundException(string message = "No encontrado.")
    : AppException("NOT_FOUND", message);
