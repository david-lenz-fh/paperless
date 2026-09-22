using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using business_layer.Interfaces;

namespace api
{
    [Route("api/[controller]")]
    [ApiController]
    public class DocumentController : ControllerBase
    {
        private readonly IDocumentService _documentService;

        public DocumentController(IDocumentService docService)
        {
            _documentService = docService;
        }
    }
}
