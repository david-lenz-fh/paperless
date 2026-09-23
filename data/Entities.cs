namespace data.Entities
{
    public class Document
    {
        public int Id { get; private set; }
        public string Filename { get; private set; }

        public Document(string filename)
        {
            Filename = filename;
        }
        public Document(int id, string filename)
        {
            Id = id;
            Filename = filename;
        }
        private Document()
        {
        }
    }

    public class FileVersion
    {
        public int Id { get; private set; }
        public int DocumentId { get; private set; }
        public Document Document { get; private set; } = null!;
        public string AiSummary { get; private set; }
        public string MimeType { get; private set; }
        public long FileSizeInBytes { get; private set; }
        public string FileHash { get; private set; }
        public string StoragePath { get; private set; }
        public DateTime UploadDate { get; private set; }
        public int Version { get; private set; }

        public FileVersion(
            int documentId,
            string mimetype,
            long fileSizeInBytes,
            string fileHash,
            string storagePath,
            string aiSummary,
            DateTime uploadDate,
            int version)
        {
            DocumentId = documentId;
            MimeType = mimetype;
            FileSizeInBytes = fileSizeInBytes;
            FileHash = fileHash;
            StoragePath = storagePath;
            AiSummary = aiSummary;
            UploadDate = uploadDate;
            Version = version;
        }

        private FileVersion()
        {
        }
    }
}