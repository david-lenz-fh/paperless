namespace business_layer.Exceptions;

public class DocumentNotFoundException : BusinessException
{
    public int DocumentId { get; }

    public DocumentNotFoundException(int documentId)
        : base($"Document with ID {documentId} was not found.")
    {
        DocumentId = documentId;
    }
}