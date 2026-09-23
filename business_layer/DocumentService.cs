using System;
using System.Collections.Generic;
using System.Text;
using business_layer.Dtos;
using data.Interfaces;
using data.Entities;

namespace business_layer
{
    public class DocumentService
    {

        private readonly IDocumentRepository _documentRepository;

        public DocumentService(IDocumentRepository docRepo)
        {
            _documentRepository = docRepo;
        }

        public async Task<int> UploadFile(FileUploadDto uploadedFileDto)
        {
            // create docoment entity
            var documentEntity = new Document(
                filename: uploadedFileDto.Title
            );
            // safe document in db, get document id
            int newDocumentId = await _documentRepository.CreateDocument(documentEntity);
            // 3fill with temporary values
            var dummyMimeType = "application/pdf";
            var dummyFileSize = 1024567L; // 1MB
            var dummyHash = "e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855";
            var dummyStoragePath = $"/var/paperless/storage/dummy_{newDocumentId}.pdf";
            var dummySummary = "KI-Zusammenfassung wird noch generiert...";
            // create file version with temporary values
            var fileVersionEntity = new FileVersion(
                documentId: newDocumentId,
                mimetype: dummyMimeType,
                fileSizeInBytes: dummyFileSize,
                fileHash: dummyHash,
                storagePath: dummyStoragePath,
                aiSummary: dummySummary,
                uploadDate: DateTime.UtcNow,
                version: 1
            );
            // add file version to repo
            await _documentRepository.CreateFile(fileVersionEntity);
            // return id
            return newDocumentId;
        }
        
        public async Task<IEnumerable<DocumentDto>> GetAllDocuments()
        {
            // get entities from dal
            var documents = await _documentRepository.GetAllDocuments();

            // map to dto
            return documents.Select(doc => new DocumentDto
            {
                Id = doc.Id,
                Filename = doc.Filename
            });
        }
        
        public async Task<DocumentDto?> GetDocumentById(int id)
        {
            // get document from db
            var document = await _documentRepository.GetDocumentById(id);
            // check if document was found
            if (document == null)
            {
                return null;
            }
            // transfer to dto hand to controller
            return new DocumentDto
            {
                Id = document.Id,
                Filename = document.Filename
            };
        }
        
        public async Task<bool> DeleteDocument(int id)
        {
            var document = await _documentRepository.GetDocumentById(id);
            if (document == null) return false;

            // get all fileversions from document
            var fileVersions = await _documentRepository.GetFilesByDocumentId(id);

            // later delete all fileversions
            foreach (var file in fileVersions)
            {
                // !!!!!!!!!!!!! TO DO: DELETE ALL FILE VERSIONS FROM STORAGE !!!!!!!!!!
            }

            // delete document and file versiosn from db
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
    }
}
