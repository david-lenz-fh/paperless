using System;
using System.Collections.Generic;
using System.Text;
using business_layer.Dtos;
using data.Interfaces;
using data.Entities;
using business_layer.Interfaces;

namespace business_layer
{
    public class DocumentService:IDocumentService
    {

        private readonly IDocumentRepository _documentRepository;

        public DocumentService(IDocumentRepository docRepo)
        {
            _documentRepository = docRepo;
        }

        public async Task<int> UploadDocument(DocumentUploadDto uploadedDocumentDto)
        {

            var documentEntity = new Document(
                filename: uploadedDocumentDto.Title,
                categoryId: null
            );

            int newDocumentId = await _documentRepository.CreateDocument(documentEntity);

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
            
            return newDocumentId;
        }
        
        public async Task<IEnumerable<DocumentDto>> GetAllDocuments()
        {
            var documents = await _documentRepository.GetAllDocuments();

            return documents.Select(doc => new DocumentDto
            {
                Id = doc.Id,
                Filename = doc.Filename
            });
        }
        
        public async Task<DocumentDto?> GetDocumentById(int id)
        {
            var document = await _documentRepository.GetDocumentById(id);
            if (document == null)
            {
                return null;
            }
            return new DocumentDto
            {
                Id = document.Id,
                Filename = document.Filename
            };
        }
        
        public async Task<bool> DeleteDocument(int id)
        {
            var document = await _documentRepository.GetDocumentById(id);
            if (document == null)
            {
                return false;
            }
            return await _documentRepository.DeleteDocument(document);
        }
        
        public async Task<IEnumerable<FileVersionDto>> GetFilesByDocumentId(int documentId)
        {
            // get all fileversions for document id
            var files = await _documentRepository.GetFilesByDocumentId(documentId);

            // transfer to dto 
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
            // get id from document
            int documentId = documentToUpdate.Id;
            
            // check if document exists
            var existingDocument = await _documentRepository.GetDocumentById(documentId);
            if (existingDocument == null)
            {
                throw new KeyNotFoundException($"Dokument mit ID {documentId} existiert nicht.");
            }

            // change title
            if (!string.IsNullOrWhiteSpace(documentToUpdate.Title) && existingDocument.Filename != documentToUpdate.Title)
            {
                existingDocument.Filename = documentToUpdate.Title;
                await _documentRepository.UpdateDocument(existingDocument);
            }
            
            //get all previous version end determine version number
            var existingFiles = (await _documentRepository.GetFilesByDocumentId(documentId)).ToList();
            int nextVersion = existingFiles.Any() ? existingFiles.Max(f => f.Version) + 1 : 1;
            
            // 3fill with temporary values
            var dummyMimeType = "application/pdf";
            var dummyFileSize = 1024567L; // 1MB
            var dummyHash = "e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855";
            var dummyStoragePath = $"/var/paperless/storage/dummy_{documentId}.pdf";
            var dummySummary = "KI-Zusammenfassung wird noch generiert...";
            // create file version with temporary values
            var fileVersionEntity = new FileVersion(
                documentId: documentId,
                mimeType: dummyMimeType,
                fileSizeInBytes: dummyFileSize,
                fileHash: dummyHash,
                storagePath: dummyStoragePath,
                aiSummary: dummySummary,
                version: nextVersion
            );
            return await _documentRepository.CreateFile(fileVersionEntity);
        }
    }
}
