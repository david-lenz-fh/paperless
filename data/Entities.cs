namespace data.Entities
{
    public class Document
    {
        public int Id { get; private set; }
        public string Filename { get; private set; } = string.Empty;

        public int? CategoryId { get; private set; }
        public DocumentCategory? Category { get; private set; }

        public Document(string filename, int? categoryId)
        {
            Filename = filename;
            CategoryId = categoryId;
        }

        private Document() { }
    }
    public class DocumentCategory
    {
        public int Id { get; private set; }

        public string CategoryName { get; private set; } = string.Empty;

        public DocumentCategory(string categoryName)
        {
            CategoryName = categoryName;
        }

        private DocumentCategory()
        {
        }
    }
    public class FileVersion
    {
        public int Id { get; private set; }

        public int DocumentId { get; private set; }

        public Document Document { get; private set; } = null!;

        public string AiSummary { get; private set; } = string.Empty;

        public string MimeType { get; private set; } = string.Empty;

        public long FileSizeInBytes { get; private set; }

        public string FileHash { get; private set; } = string.Empty;

        public string StoragePath { get; private set; } = string.Empty;

        public DateTime UploadDate { get; private set; }

        public int Version { get; private set; }

        public FileVersion(
            int documentId,
            string mimeType,
            long fileSizeInBytes,
            string fileHash,
            string storagePath,
            string aiSummary,
            int version)
        {
            DocumentId = documentId;
            MimeType = mimeType;
            FileSizeInBytes = fileSizeInBytes;
            FileHash = fileHash;
            StoragePath = storagePath;
            AiSummary = aiSummary;
            UploadDate = DateTime.UtcNow;
            Version = version;
        }

        private FileVersion()
        {
        }
    }
}