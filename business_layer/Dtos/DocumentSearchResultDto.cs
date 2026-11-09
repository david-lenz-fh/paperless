using System;

namespace business_layer.Dtos
{
    public class DocumentSearchResultDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Filename { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public double Score { get; set; } // Relevanz-Score von Elasticsearch
    }
}