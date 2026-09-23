using business_layer.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace business_layer.Interfaces
{
    public interface IDocumentService
    {

        public Document GetDocumentById(Guid Id);
        public IEnumerable<Document> GetDocuments();
        public Guid UploadFile(RawFile uploadedFile);
        public RawFile DownloadFile(Guid id);
    }

}
