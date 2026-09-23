using business_layer.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using business_layer.Dtos;

namespace business_layer.Interfaces
{
    public interface IDocumentService
    {

        public Task<DocumentDto?> GetDocumentById(int id);
        public Task<IEnumerable<DocumentDto>> GetAllDocuments();
        public Task<int> UploadFile(FileUploadDto uploadedFile);

        public Task<bool> DeleteDocument(int id);

        public Task<IEnumerable<FileVersionDto>> GetFilesByDocumentId(int documentId);
        //public RawFile DownloadFile(Guid id);
    }
}
