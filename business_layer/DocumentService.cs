using System;
using System.Collections.Generic;
using System.Text;
using data.Interfaces;

namespace business_layer
{
    public class DocumentService
    {

        private readonly IDocumentRepository _documentRepository;
        public DocumentService(IDocumentRepository docRepo)
        {
            _documentRepository = docRepo;
        }
    }
}
