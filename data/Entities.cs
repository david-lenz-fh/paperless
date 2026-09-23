using System;
using System.Collections.Generic;
using System.Text;

namespace data.Entities
{
    public class Document
    {
        public Guid Id { get; private set; }
        public Guid FileId { get; private set; }
        public DocumentFile File { get; private set; }

        public string Filename { get; private set; }
        public string Summary { get; private set; }
        public DateTime Created { get; private set; }
        public DateTime LastUpdated { get; private set; }

        public Document(
            Guid id,
            Guid fileId,
            string filename,
            string summary,
            DateTime created,
            DateTime lastUpdated)
        {
            Id = id;
            FileId = fileId;
            Filename = filename;
            Summary = summary;
            Created = created;
            LastUpdated = lastUpdated;
        }
        private Document() { }
    }
    public class DocumentFile
    {
        public Guid Id { get; private set; }

        public string MimeType { get; private set; }
        public long FileSizeInBytes { get; private set; }
        public string FileHash { get; private set; }
        public string StoragePath { get; private set; }

        public DocumentFile(Guid id, string mimetype, long fileSizeInBytes, string fileHash, string storagePath)
        {
            Id = id;
            MimeType = mimetype;
            FileSizeInBytes = fileSizeInBytes;
            FileHash = fileHash;
            StoragePath = storagePath;
        }
        private DocumentFile() { }
    }
    
}
    