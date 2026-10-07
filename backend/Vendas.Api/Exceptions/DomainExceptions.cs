namespace Vendas.Api.Exceptions;

public abstract class DomainException(string message, int statusCode) : Exception(message)
{
    public int StatusCode { get; } = statusCode;
}

public class NotFoundException(string message) : DomainException(message, 404);

public class ConflictException(string message) : DomainException(message, 409);

public class BadRequestException(string message) : DomainException(message, 400);
