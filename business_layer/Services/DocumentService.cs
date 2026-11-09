using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
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
        private readonly IFileStorageService _fileStorageService;
        private readonly ILogger<DocumentService> _logger;

        public DocumentService(
            IDocumentRepository docRepo,
            IFileStorageService fileStorageService,
            ILogger<DocumentService> logger)
        {
            _documentRepository = docRepo;
            _fileStorageService = fileStorageService;
            _logger = logger;
        }

        public async Task<FileVersionDto> UploadDocument(DocumentUploadDto uploadedDocumentDto)
        {
            _logger.LogInformation("Starte Upload-Prozess für Dokument mit Titel: {Title}", uploadedDocumentDto.Title);

            if (string.IsNullOrWhiteSpace(uploadedDocumentDto.Title))
            {
                _logger.LogWarning("Upload abgelehnt: Der Dokumententitel ist leer.");
                throw new DocumentValidationException("Dokumententitel darf nicht leer sein.");
            }

            if (uploadedDocumentDto.FileStream == null || uploadedDocumentDto.FileSize <= 0)
            {
                _logger.LogWarning("Upload abgelehnt: Keine gültige Datei für '{Title}' übergeben.", uploadedDocumentDto.Title);
                throw new DocumentValidationException("Es muss eine gültige Datei hochgeladen werden.");
            }

            try
            {
                var documentEntity = new Document(
                    filename: uploadedDocumentDto.Title,
                    categoryId: null
                );

                int newDocumentId = await _documentRepository.CreateDocument(documentEntity);
                _logger.LogInformation("Dokument erfolgreich mit ID {DocumentId} in DB angelegt.", newDocumentId);

                // SHA-256 Hash berechnen
                using var sha256 = SHA256.Create();
                byte[] hashBytes = await sha256.ComputeHashAsync(uploadedDocumentDto.FileStream);
                string fileHash = Convert.ToHexString(hashBytes).ToLowerInvariant();

                if (uploadedDocumentDto.FileStream.CanSeek)
                {
                    uploadedDocumentDto.FileStream.Position = 0;
                }

                string extension = Path.GetExtension(uploadedDocumentDto.OriginalFileName);
                if (string.IsNullOrWhiteSpace(extension))
                {
                    extension = ".pdf";
                }

                string objectName = $"doc_{newDocumentId}_v1{extension}";

                // Datei in MinIO speichern
                string storagePath = await _fileStorageService.UploadFileAsync(
                    objectName,
                    uploadedDocumentDto.FileStream,
                    uploadedDocumentDto.FileSize,
                    uploadedDocumentDto.ContentType
                );

                _logger.LogInformation("Datei für Dokument ID {DocumentId} in MinIO unter '{StoragePath}' gespeichert.",
                    newDocumentId, storagePath);


                var fileVersionEntity = new FileVersion(
                    documentId: newDocumentId,
                    mimeType: uploadedDocumentDto.ContentType,
                    fileSizeInBytes: uploadedDocumentDto.FileSize,
                    fileHash: fileHash,
                    storagePath: storagePath,
                    aiSummary: "KI-Zusammenfassung wird noch generiert...",
                    version: 1
                );

                int fileVersionId = await _documentRepository.CreateFile(fileVersionEntity);
                _logger.LogInformation("Initiale Dateiversion (Version 1) für Dokument ID {DocumentId} erfolgreich in DB erstellt.", newDocumentId);

                var fileVersionDto = new FileVersionDto
                {
                    Id = fileVersionId, 
                    DocumentId = newDocumentId,
                    MimeType = uploadedDocumentDto.ContentType,
                    FileSizeInBytes = uploadedDocumentDto.FileSize,
                    FileHash = fileHash,
                    StoragePath = storagePath,
                    AiSummary = fileVersionEntity.AiSummary,
                    UploadDate = DateTime.UtcNow, 
                    Version = 1
                };

                return fileVersionDto;
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

            var files = await _documentRepository.GetFilesByDocumentId(id);
            foreach (var file in files)
            {
                if (!string.IsNullOrWhiteSpace(file.StoragePath))
                {
                    var objectName = file.StoragePath.Contains('/')
                        ? file.StoragePath.Split('/').Last()
                        : file.StoragePath;

                    await _fileStorageService.DeleteFileAsync(objectName);
                }
            }

            bool isDeleted = await _documentRepository.DeleteDocument(document);
            _logger.LogInformation("Dokument mit ID {DocumentId} erfolgreich gelöscht.", id);

            return isDeleted;
        }

        public async Task<IEnumerable<FileVersionDto>> GetFilesByDocumentId(int documentId)
        {
            _logger.LogDebug("Rufe Dateiversionen für Dokument ID {DocumentId} ab.", documentId);

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

            if (!string.IsNullOrWhiteSpace(documentToUpdate.Title) && existingDocument.Filename != documentToUpdate.Title)
            {
                _logger.LogInformation("Aktualisiere Titel für Dokument {DocumentId} von '{OldTitle}' zu '{NewTitle}'.",
                    documentId, existingDocument.Filename, documentToUpdate.Title);

                existingDocument.Filename = documentToUpdate.Title;
                await _documentRepository.UpdateDocument(existingDocument);
            }

            var existingFiles = (await _documentRepository.GetFilesByDocumentId(documentId)).ToList();
            int nextVersion = existingFiles.Any() ? existingFiles.Max(f => f.Version) + 1 : 1;
            var lastFile = existingFiles.OrderByDescending(f => f.Version).FirstOrDefault();

            var fileVersionEntity = new FileVersion(
                documentId: documentId,
                mimeType: lastFile?.MimeType ?? "application/pdf",
                fileSizeInBytes: lastFile?.FileSizeInBytes ?? 0L,
                fileHash: lastFile?.FileHash ?? string.Empty,
                storagePath: lastFile?.StoragePath ?? string.Empty,
                aiSummary: lastFile?.AiSummary ?? "KI-Zusammenfassung wird noch generiert...",
                version: nextVersion
            );

            int fileVersionId = await _documentRepository.CreateFile(fileVersionEntity);
            _logger.LogInformation("Dateiversion {Version} (ID {FileVersionId}) für Dokument {DocumentId} erfolgreich persistiert.",
                nextVersion, fileVersionId, documentId);

            return fileVersionId;
        }

        public async Task<DocumentDownloadDto> DownloadDocument(int id)
        {
            _logger.LogInformation("Starte Download für Dokument ID {DocumentId}.", id);

            var document = await _documentRepository.GetDocumentById(id);
            if (document == null)
            {
                _logger.LogWarning("Download fehlgeschlagen: Dokument mit ID {DocumentId} existiert nicht.", id);
                throw new DocumentNotFoundException(id);
            }

            var files = (await _documentRepository.GetFilesByDocumentId(id)).ToList();
            var latestFile = files.OrderByDescending(f => f.Version).FirstOrDefault();

            if (latestFile == null || string.IsNullOrWhiteSpace(latestFile.StoragePath))
            {
                _logger.LogWarning("Download fehlgeschlagen: Keine Dateiversion für Dokument ID {DocumentId} gefunden.", id);
                throw new DocumentNotFoundException(id);
            }

            var objectName = latestFile.StoragePath.Contains('/')
                ? latestFile.StoragePath.Split('/').Last()
                : latestFile.StoragePath;

            var fileStream = await _fileStorageService.DownloadFileAsync(objectName);

            // Dateiname sicherstellen (falls der User als Titel keine Endung wie .pdf eingegeben hat)
            var downloadFileName = document.Filename;
            var extension = Path.GetExtension(objectName);
            if (!string.IsNullOrEmpty(extension) && !downloadFileName.EndsWith(extension, StringComparison.OrdinalIgnoreCase))
            {
                downloadFileName += extension;
            }

            _logger.LogInformation("Datei '{ObjectName}' für Dokument ID {DocumentId} erfolgreich aus Storage geladen.", objectName, id);

            return new DocumentDownloadDto(
                fileStream: fileStream,
                contentType: latestFile.MimeType,
                fileName: downloadFileName
            );
        }
    }
}