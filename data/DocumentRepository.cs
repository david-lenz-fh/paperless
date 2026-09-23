using data.Entities;
using data.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;

namespace data
{
    public class DocumentRepository : IDocumentRepository
    {
        private readonly PaperlessDbContext _context;
        public DocumentRepository(PaperlessDbContext context)
        {
            _context = context;
        }
        
        // add document
        public async Task<int> CreateDocument(Document doc)
        {
            await _context.Documents.AddAsync(doc);
            await _context.SaveChangesAsync();

            return doc.Id;
        }
        
        // add file version 
        public async Task<int> CreateFile(FileVersion file)
        {
            await _context.Files.AddAsync(file);
            await _context.SaveChangesAsync();

            return file.Id;
        }
        
        // delete a document 
        public async Task<bool> DeleteDocument(Document doc)
        {
            _context.Documents.Remove(doc);
            int rowsAffected = await _context.SaveChangesAsync();
;           return rowsAffected > 0;
        }

        //get all documents
        public async Task<IEnumerable<Document>> GetAllDocuments()
        {
            return await _context.Documents.ToListAsync();
        }
        // get all fileVersions
        public async Task<IEnumerable<FileVersion>> GetAllFiles()
        {
            return await _context.Files.ToListAsync();
        }

        public async Task<Document?> GetDocumentById(int id)
        {
            return await _context.Documents.FirstOrDefaultAsync(d => d.Id == id);
        }
        public Task<bool> DeleteFile(FileVersion file)
        {
            throw new NotImplementedException();
        }
        

        public async Task<IEnumerable<FileVersion>> GetFilesByDocumentId(int documentId)
        {
            return await _context.Files
                .Where(f => f.DocumentId == documentId)
                .ToListAsync();
        }

        public Task<bool> UpdateDocument(Document doc)
        {
            throw new NotImplementedException();
        }
    }
}
