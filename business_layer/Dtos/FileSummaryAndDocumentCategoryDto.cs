using System;
using System.Collections.Generic;
using System.Text;

namespace business_layer.Dtos
{
    public class FileSummaryAndDocumentCategoryDto
    {
        public int DocumentId { get; set; }
        public int FileId { get; set; }
        public string Summary { get; set; }
        public string Category { get; set; }

        public FileSummaryAndDocumentCategoryDto(int documentId, int fileId, string summary, string category)
        {
            DocumentId = documentId;
            FileId = fileId;
            Summary = summary;
            Category = category;
        }
    }
}
