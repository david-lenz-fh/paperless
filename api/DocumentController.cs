using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using business_layer.Interfaces;
using api.Dtos;

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
        [HttpGet]
        public IActionResult GetAllDocuments()
        {
            return Ok("Hello World");
        }

        [HttpGet("{id:guid}")]
        public IActionResult GetDocumentById(Guid Id)
        {
            return Ok("Hello World");
        }

        [HttpPost]
        public async Task<IActionResult> UploadFile(UploadDocumentRequest document)
        {
            if (document.file == null || document.file.Length == 0)
            {
                return BadRequest("No file uploaded.");
            }
            return Ok("Hi");
        }

        [HttpGet("{id}/download")]
        public async Task<IActionResult> DownloadFile(Guid id)
        {
            return Ok();
        }
    }
}
