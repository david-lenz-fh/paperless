using business_layer.Dtos;
using business_layer.Interfaces;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace api
{
    [Route("api/[controller]")]
    [ApiController]
    [EnableCors("AllowAll")]
    public class DocumentController : ControllerBase
    {
        private readonly IDocumentService _documentService;
        private readonly ILogger<DocumentController> _logger;

        public DocumentController(IDocumentService docService, ILogger<DocumentController> logger)
        {
            _documentService = docService;
            _logger = logger;
        }

        [HttpPost]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UploadDocument([FromForm] string title, IFormFile file)
        {
            _logger.LogInformation("HTTP POST /api/Document empfangen mit Titel: '{Title}', Dateigröße: {FileSize} Bytes.",
                title, file?.Length ?? 0);

            if (string.IsNullOrWhiteSpace(title))
            {
                _logger.LogWarning("HTTP POST abgelehnt: Titel darf nicht leer sein.");
                return BadRequest("Titel darf nicht leer sein.");
            }

            if (file == null || file.Length == 0)
            {
                _logger.LogWarning("HTTP POST abgelehnt: Keine Datei übermittelt.");
                return BadRequest("No file uploaded.");
            }

            var fileDto = new DocumentUploadDto(title);
            var newId = await _documentService.UploadDocument(fileDto);

            _logger.LogInformation("Sende neu erstellte Dokument-ID {DocumentId} an RabbitMQ Queue.", newId);
            await RabbitService.SendToQueue(newId.ToString());

            return Ok(new { Id = newId, Message = "Dokument erfolgreich hochgeladen." });
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<DocumentDto>>> GetAllDocuments()
        {
            _logger.LogInformation("HTTP GET /api/Document empfangen.");
            var documents = await _documentService.GetAllDocuments();
            return Ok(documents);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<DocumentDto>> GetDocumentById(int id)
        {
            _logger.LogInformation("HTTP GET /api/Document/{Id} empfangen.", id);
            // Wirft DocumentNotFoundException -> Middleware übersetzt zu 404
            var document = await _documentService.GetDocumentById(id);
            return Ok(document);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDocument(int id)
        {
            _logger.LogInformation("HTTP DELETE /api/Document/{Id} empfangen.", id);
            // Wirft DocumentNotFoundException -> Middleware übersetzt zu 404
            await _documentService.DeleteDocument(id);
            return NoContent();
        }

        [HttpGet("{documentId}/files")]
        public async Task<IActionResult> GetFilesByDocumentId(int documentId)
        {
            _logger.LogInformation("HTTP GET /api/Document/{DocumentId}/files empfangen.", documentId);
            // Wirft DocumentNotFoundException -> Middleware übersetzt zu 404
            var files = await _documentService.GetFilesByDocumentId(documentId);
            return Ok(files);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateDocument(int id, [FromBody] DocumentUpdateDto documentToUpdate)
        {
            _logger.LogInformation("HTTP PUT /api/Document/{Id} empfangen.", id);

            if (documentToUpdate == null || string.IsNullOrWhiteSpace(documentToUpdate.Title))
            {
                _logger.LogWarning("HTTP PUT abgelehnt: Ungültige Daten oder leerer Titel.");
                return BadRequest("Ungültige Daten oder Titel ist leer.");
            }

            documentToUpdate.Id = id;

            // Wirft DocumentNotFoundException -> Middleware übersetzt zu 404
            int fileVersionId = await _documentService.UpdateDocument(documentToUpdate);
            return Ok(new { FileVersionId = fileVersionId, Message = "Dokument erfolgreich aktualisiert." });
        }
    }
}