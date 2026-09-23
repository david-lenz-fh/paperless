using data.Entities;
using data.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace data
{
    public class DocumentRepository : IDocumentRepository
    {
        private readonly PaperlessDbContext _context;
        public DocumentRepository(PaperlessDbContext context)
        {
            _context = context;
        }

        public async Task<int> CreateDocument(Document doc)
        {
            await _context.Documents.AddAsync(doc);
            await _context.SaveChangesAsync();

            return doc.Id;
        }

        public async Task<int> CreateFile(FileVersion file)
        {
            await _context.Files.AddAsync(file);
            await _context.SaveChangesAsync();

            return file.Id;
        }

        public async Task<bool> DeleteDocument(Document doc)
        {
            _context.Documents.Remove(doc);
            int rowsAffected = await _context.SaveChangesAsync();

;           return rowsAffected > 0;
        }

        public Task<bool> DeleteFile(FileVersion file)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<Document>> GetAllDocuments()
        {
            throw new NotImplementedException();
        }

        public Task<Document> GetDocumentById(Guid id)
        {
            throw new NotImplementedException();
        }

        public Task<Document> GetDocumentById(int id)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<FileVersion>> GetFilesByDocumentId(int document_id)
        {
            throw new NotImplementedException();
        }

        public Task<bool> UpdateDocument(Document doc)
        {
            throw new NotImplementedException();
        }
    }
}
