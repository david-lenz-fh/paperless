namespace business_layer.Exceptions;

public sealed class ExternalServiceException : BusinessException
{
    public ExternalServiceException(string serviceName, Exception innerException)
        : base($"The external service '{serviceName}' is currently unavailable.", innerException)
    {
    }
}
