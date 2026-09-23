using System.ComponentModel.DataAnnotations;

namespace business_layer.Dtos;

public class FileUploadDto
{
    [Required] public string Title { get; set; } = string.Empty;
}