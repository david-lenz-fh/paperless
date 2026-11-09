using System.IO;

namespace business_layer.Dtos
{
    public class DocumentDownloadDto
    {
        public Stream FileStream { get; set; }
        public string ContentType { get; set; }
        public string FileName { get; set; }

        public DocumentDownloadDto(Stream fileStream, string contentType, string fileName)
        {
            FileStream = fileStream;
            ContentType = contentType;
            FileName = fileName;
        }
    }
}