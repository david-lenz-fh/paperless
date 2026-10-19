using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using business_layer.Dtos;
using business_layer.Exceptions;
using business_layer.Interfaces;
using data.Entities;
using data.Interfaces;
using Microsoft.Extensions.Logging;

namespace business_layer
{
    public class DocumentService : IDocumentService
    {
        private readonly IDocumentRepository _documentRepository;
        private readonly ILogger<DocumentService> _logger;

        public DocumentService(IDocumentRepository docRepo, ILogger<DocumentService> logger)
        {
            _documentRepository = docRepo;
            _logger = logger;
        }

        public async Task<int> UploadDocument(DocumentUploadDto uploadedDocumentDto)
        {
            _logger.LogInformation("Starte Upload-Prozess für Dokument mit Titel: {Title}", uploadedDocumentDto.Title);

            if (string.IsNullOrWhiteSpace(uploadedDocumentDto.Title))
            {
                _logger.LogWarning("Upload abgelehnt: Der Dokumententitel ist leer.");
                throw new DocumentValidationException("Dokumententitel darf nicht leer sein.");
            }

            try
            {
                var documentEntity = new Document(
                    filename: uploadedDocumentDto.Title,
                    categoryId: null
                );

                int newDocumentId = await _documentRepository.CreateDocument(documentEntity);
                _logger.LogInformation("Dokument erfolgreich mit ID {DocumentId} in DB angelegt.", newDocumentId);

                // Placeholder Values
                var dummyMimeType = "application/pdf";
                var dummyFileSize = 1024567L; // 1MB
                var dummyHash = "e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855";
                var dummyStoragePath = $"/var/paperless/storage/dummy_{newDocumentId}.pdf";
                var dummySummary = "KI-Zusammenfassung wird noch generiert...";

                var fileVersionEntity = new FileVersion(
                    documentId: newDocumentId,
                    mimeType: dummyMimeType,
                    fileSizeInBytes: dummyFileSize,
                    fileHash: dummyHash,
                    storagePath: dummyStoragePath,
                    aiSummary: dummySummary,
                    version: 1
                );

                await _documentRepository.CreateFile(fileVersionEntity);
                _logger.LogInformation("Initiale Dateiversion (Version 1) für Dokument ID {DocumentId} erfolgreich erstellt.", newDocumentId);

                return newDocumentId;
            }
            catch (Exception ex) when (ex is not BusinessException)
            {
                _logger.LogError(ex, "Unerwarteter Fehler beim Hochladen des Dokuments '{Title}'.", uploadedDocumentDto.Title);
                throw;
            }
        }

        public async Task<IEnumerable<DocumentDto>> GetAllDocuments()
        {
            _logger.LogDebug("Rufe alle Dokumente ab.");
            var documents = await _documentRepository.GetAllDocuments();

            return documents.Select(doc => new DocumentDto
            {
                Id = doc.Id,
                Filename = doc.Filename
            });
        }

        public async Task<DocumentDto?> GetDocumentById(int id)
        {
            _logger.LogDebug("Rufe Dokument mit ID {DocumentId} ab.", id);
            var document = await _documentRepository.GetDocumentById(id);

            if (document == null)
            {
                _logger.LogWarning("Dokument mit ID {DocumentId} wurde nicht gefunden.", id);
                throw new DocumentNotFoundException(id);
            }

            return new DocumentDto
            {
                Id = document.Id,
                Filename = document.Filename
            };
        }

        public async Task<bool> DeleteDocument(int id)
        {
            _logger.LogInformation("Versuche Dokument mit ID {DocumentId} zu löschen.", id);

            var document = await _documentRepository.GetDocumentById(id);
            if (document == null)
            {
                _logger.LogWarning("Löschen fehlgeschlagen: Dokument mit ID {DocumentId} existiert nicht.", id);
                throw new DocumentNotFoundException(id);
            }

            bool isDeleted = await _documentRepository.DeleteDocument(document);
            if (isDeleted)
            {
                _logger.LogInformation("Dokument mit ID {DocumentId} erfolgreich gelöscht.", id);
            }
            else
            {
                _logger.LogError("Löschvorgang für Dokument mit ID {DocumentId} im Repository fehlgeschlagen.", id);
            }

            return isDeleted;
        }

        public async Task<IEnumerable<FileVersionDto>> GetFilesByDocumentId(int documentId)
        {
            _logger.LogDebug("Rufe Dateiversionen für Dokument ID {DocumentId} ab.", documentId);

            // Prüfen, ob das Dokument überhaupt existiert
            var document = await _documentRepository.GetDocumentById(documentId);
            if (document == null)
            {
                _logger.LogWarning("Abruf der Dateiversionen fehlgeschlagen: Dokument mit ID {DocumentId} existiert nicht.", documentId);
                throw new DocumentNotFoundException(documentId);
            }

            var files = await _documentRepository.GetFilesByDocumentId(documentId);

            return files.Select(f => new FileVersionDto
            {
                Id = f.Id,
                DocumentId = f.DocumentId,
                MimeType = f.MimeType,
                FileSizeInBytes = f.FileSizeInBytes,
                FileHash = f.FileHash,
                StoragePath = f.StoragePath,
                AiSummary = f.AiSummary,
                UploadDate = f.UploadDate,
                Version = f.Version
            });
        }

        public async Task<int> UpdateDocument(DocumentUpdateDto documentToUpdate)
        {
            int documentId = documentToUpdate.Id;
            _logger.LogInformation("Starte Aktualisierung für Dokument ID {DocumentId}.", documentId);

            var existingDocument = await _documentRepository.GetDocumentById(documentId);
            if (existingDocument == null)
            {
                _logger.LogWarning("Aktualisierung fehlgeschlagen: Dokument mit ID {DocumentId} nicht gefunden.", documentId);
                throw new DocumentNotFoundException(documentId);
            }

            // Titel aktualisieren, falls geändert
            if (!string.IsNullOrWhiteSpace(documentToUpdate.Title) && existingDocument.Filename != documentToUpdate.Title)
            {
                _logger.LogInformation("Aktualisiere Titel für Dokument {DocumentId} von '{OldTitle}' zu '{NewTitle}'.",
                    documentId, existingDocument.Filename, documentToUpdate.Title);

                existingDocument.Filename = documentToUpdate.Title;
                await _documentRepository.UpdateDocument(existingDocument);
            }

            // Nächste Versionsnummer bestimmen
            var existingFiles = (await _documentRepository.GetFilesByDocumentId(documentId)).ToList();
            int nextVersion = existingFiles.Any() ? existingFiles.Max(f => f.Version) + 1 : 1;

            _logger.LogInformation("Erstelle neue Dateiversion {Version} für Dokument ID {DocumentId}.", nextVersion, documentId);

            var dummyMimeType = "application/pdf";
            var dummyFileSize = 1024567L;
            var dummyHash = "e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855";
            var dummyStoragePath = $"/var/paperless/storage/dummy_{documentId}.pdf";
            var dummySummary = "KI-Zusammenfassung wird noch generiert...";

            var fileVersionEntity = new FileVersion(
                documentId: documentId,
                mimeType: dummyMimeType,
                fileSizeInBytes: dummyFileSize,
                fileHash: dummyHash,
                storagePath: dummyStoragePath,
                aiSummary: dummySummary,
                version: nextVersion
            );

            int fileVersionId = await _documentRepository.CreateFile(fileVersionEntity);
            _logger.LogInformation("Dateiversion {Version} (ID {FileVersionId}) für Dokument {DocumentId} erfolgreich persistiert.",
                nextVersion, fileVersionId, documentId);

            return fileVersionId;
        }
    }
}