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
        
        // add document
        public async Task<int> CreateDocument(Document doc)
        {
            await _context.Documents.AddAsync(doc);
            await _context.SaveChangesAsync();

            return doc.Id;
        }
        
        // update a document 
        public async Task<bool> UpdateDocument(Document doc)
        {
            _context.Documents.Update(doc);
            int rowsAffected = await _context.SaveChangesAsync();
            return rowsAffected > 0;
        }
        
        // add file version 
        public async Task<int> CreateFile(FileVersion file)
        {
            await _context.FileVersions.AddAsync(file);
            await _context.SaveChangesAsync();

            return file.Id;
        }
        
        // delete a documents and files of that document
        public async Task<bool> DeleteDocument(Document doc)
        {
            _context.Documents.Remove(doc);
            int rowsAffected = await _context.SaveChangesAsync();
            return rowsAffected > 0;
        }

        //get all documents
        public async Task<IEnumerable<Document>> GetAllDocuments()
        {
            return await _context.Documents.ToListAsync();
        }
        public async Task<IEnumerable<FileVersion>> GetAllFiles()
        {
            return await _context.FileVersions.ToListAsync();
        }

        public async Task<Document?> GetDocumentById(int id)
        {
            return await _context.Documents.FirstOrDefaultAsync(d => d.Id == id);
        }
        public async Task<bool> DeleteFile(FileVersion file)
        {
            _context.FileVersions.Remove(file);
            int rowsAffected = await _context.SaveChangesAsync();
            
            return rowsAffected > 0;
        }
        

        public async Task<IEnumerable<FileVersion>> GetFilesByDocumentId(int documentId)
        {
            return await _context.FileVersions
                .Where(f => f.DocumentId == documentId)
                .ToListAsync();
        }
        

    }
}
