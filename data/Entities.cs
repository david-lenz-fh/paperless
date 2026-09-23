using System;
using System.Collections.Generic;
using System.Text;

namespace data.Entities
{
    public class Document
    {
        public int Id { get; private set; }
        public string Filename { get; private set; }
        public Document(
            int id,
            int fileId,
            string filename)
        {
            Id = id;
            Filename = filename;
        }
        private Document() { }
    }

    public class FileVersion
    {
        public int Id { get; private set; }
        public int DocumentId { get; private set; }
        public Document Document { get; private set; }
        public string AiSummary { get; private set; }
        public string MimeType { get; private set; }
        public long FileSizeInBytes { get; private set; }
        public string FileHash { get; private set; }
        public string StoragePath { get; private set; }
        public DateTime UploadDate;
        public int Version;

        public FileVersion(int id, string mimetype, long fileSizeInBytes, string fileHash, string storagePath, string aiSummary, DateTime uploadDate, int version)
        {
            Id = id;
            MimeType = mimetype;
            FileSizeInBytes = fileSizeInBytes;
            FileHash = fileHash;
            StoragePath = storagePath;
            AiSummary = aiSummary;
            UploadDate = uploadDate;
            Version = version;
        }
        private FileVersion() { }
    }
    
}
    