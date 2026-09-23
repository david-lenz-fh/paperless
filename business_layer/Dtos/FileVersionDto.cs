namespace business_layer.Dtos;

public class FileVersionDto
{
    public int Id { get; set; }
    public int DocumentId { get; set; }
    public string MimeType { get; set; } = string.Empty;
    public long FileSizeInBytes { get; set; }
    public string FileHash { get; set; } = string.Empty;
    public string StoragePath { get; set; } = string.Empty;
    public string AiSummary { get; set; } = string.Empty;
    public DateTime UploadDate { get; set; }
    public int Version { get; set; }
}