using System.ComponentModel.DataAnnotations;

namespace business_layer.Dtos;

public class DocumentUpdateDto
{
    [Required] 
    public int Id { get; set; }

    [Required] 
    public string Title { get; set; } = string.Empty;

    public DocumentUpdateDto() { }

    public DocumentUpdateDto(int id, string title)
    {
        Id = id;
        Title = title;
    }
}