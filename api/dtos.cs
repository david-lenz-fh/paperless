namespace api.Dtos
{
    public record UploadDocumentRequest(string title, IFormFile file);
}
