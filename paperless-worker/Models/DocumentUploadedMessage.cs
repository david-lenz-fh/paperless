namespace paperless_worker; // Oder der Namespace, der in deinem Projekt gilt

public record DocumentUploadedMessage(Guid DocumentId, string ObjectKey, string FileName);