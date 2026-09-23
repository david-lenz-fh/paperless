using data.Entities;
using data.Interfaces;
using Microsoft.EntityFrameworkCore;
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

        public async Task<bool> DeleteFile(FileVersion file)
        {
            _context.Files.Remove(file);
            int rowsAffected = await _context.SaveChangesAsync();
            
            return rowsAffected > 0;
        }

        public async Task<IEnumerable<Document>> GetAllDocuments()
        {
            return await _context.Documents.ToListAsync();
        }

        public async Task<Document?> GetDocumentById(int id)
        {
            return await _context.Documents.FindAsync(id);
        }

        public async Task<IEnumerable<FileVersion>> GetFilesByDocumentId(int document_id)
        {
            var files = await _context.Files.Where(file=>file.Document.Id==document_id).ToListAsync();
            if (files == null)
            {
                return Enumerable.Empty<FileVersion>();
            }

            return files;
        }

        public async Task<bool> UpdateDocument(Document doc)
        {
            _context.Documents.Update(doc);
            var affectedRows = await _context.SaveChangesAsync();

            return affectedRows > 0;
        }
    }
}
