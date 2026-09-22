using System;
using System.Collections.Generic;
using System.Text;

namespace data.Entities
{
    public record Document(Guid Id, Guid FileId, DocumentFile File, string Filename, string? Summary, DateTime created, DateTime lastUpdated);

    public record DocumentFile(Guid Id, Document Document, string MimeType, int FileSizeInBytes, string FileHash, string StoragePath);
}
    