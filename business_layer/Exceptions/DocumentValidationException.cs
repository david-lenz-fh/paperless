namespace business_layer.Exceptions;

public class DocumentValidationException : BusinessException
{
    public DocumentValidationException(string message) : base(message) { }
}