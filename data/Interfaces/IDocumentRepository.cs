using data.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace data.Interfaces
{
    public interface IDocumentRepository
    {
        public Task<int> CreateDocument(Document doc);
        public Task<int> CreateFile(FileVersion file);
        public Task<bool> UpdateDocument(Document doc);
        public Task<bool> DeleteDocument(Document doc);
        public Task<bool> DeleteFile(FileVersion file);

        public Task<IEnumerable<Document>> GetAllDocuments();
        public Task<Document?> GetDocumentById(int id);
        public Task<IEnumerable<FileVersion>> GetFilesByDocumentId(int document_id);
    }
}
