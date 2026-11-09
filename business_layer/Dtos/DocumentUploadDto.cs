using System.IO;

namespace business_layer.Dtos
{
    public class DocumentUploadDto
    {
        public string Title { get; set; }
        public Stream FileStream { get; set; }
        public string ContentType { get; set; }
        public long FileSize { get; set; }
        public string OriginalFileName { get; set; }

        public DocumentUploadDto(string title, Stream fileStream, string contentType, long fileSize, string originalFileName)
        {
            Title = title;
            FileStream = fileStream;
            ContentType = contentType;
            FileSize = fileSize;
            OriginalFileName = originalFileName;
        }
    }
}