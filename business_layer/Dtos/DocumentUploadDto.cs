using System.ComponentModel.DataAnnotations;

namespace business_layer.Dtos;

public class DocumentUploadDto
{
    [Required] public string Title { get; set; } = string.Empty;
    public DocumentUploadDto(string title)
    {
        Title = title;
    }
}