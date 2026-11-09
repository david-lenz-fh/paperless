using System;

namespace paperless_worker.Models
{
    public class DocumentIndexDocument
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Filename { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}