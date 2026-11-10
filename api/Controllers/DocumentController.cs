using business_layer.Dtos;
using business_layer.Interfaces;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace api
{
    [Route("api/[controller]")]
    [ApiController]
    [EnableCors("AllowAll")]
    public class DocumentController : ControllerBase
    {
        private readonly IDocumentService _documentService;
        private readonly ISearchService _searchService;
        private readonly ILogger<DocumentController> _logger;

        public DocumentController(IDocumentService docService, ISearchService searchService, ILogger<DocumentController> logger)
        {
            _documentService = docService;
            _searchService = searchService;
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

            await using var stream = file.OpenReadStream();
            var fileDto = new DocumentUploadDto(
                title: title,
                fileStream: stream,
                contentType: file.ContentType,
                fileSize: file.Length,
                originalFileName: file.FileName
            );

            var fileVersion = await _documentService.UploadDocument(fileDto);
            int documentId = fileVersion.DocumentId;

            var objectKey = !string.IsNullOrWhiteSpace(fileVersion.StoragePath)
                ? (fileVersion.StoragePath.Contains('/') ? fileVersion.StoragePath.Split('/').Last() : fileVersion.StoragePath)
                : $"doc_{documentId}_v1{Path.GetExtension(file.FileName)}";

            _logger.LogInformation("Sende strukturierte Nachricht für Dokument ID {DocumentId} an RabbitMQ Queue.", documentId);

            await RabbitService.SendDocumentUploadedMessageAsync(documentId, objectKey, file.FileName);

            return Ok(new { Id = documentId, Message = "Dokument erfolgreich in DB und MinIO gespeichert." });
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<DocumentDto>>> GetAllDocuments()
        {
            _logger.LogInformation("HTTP GET /api/Document empfangen.");
            var documents = await _documentService.GetAllDocuments();
            return Ok(documents);
        }

        [HttpGet("search")]
        public async Task<ActionResult<IEnumerable<DocumentSearchResultDto>>> SearchDocuments([FromQuery] string q)
        {
            _logger.LogInformation("HTTP GET /api/Document/search?q={Query} empfangen.", q);

            if (string.IsNullOrWhiteSpace(q))
            {
                return BadRequest("Der Suchparameter 'q' darf nicht leer sein.");
            }

            var results = await _searchService.SearchDocumentsAsync(q);
            return Ok(results);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<DocumentDto>> GetDocumentById(int id)
        {
            _logger.LogInformation("HTTP GET /api/Document/{Id} empfangen.", id);
            // Wirft DocumentNotFoundException -> Middleware übersetzt zu 404
            var document = await _documentService.GetDocumentById(id);
            return Ok(document);
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteDocument(int id)
        {
            _logger.LogInformation("HTTP DELETE /api/Document/{Id} empfangen.", id);
            // Wirft DocumentNotFoundException -> Middleware übersetzt zu 404
            await _documentService.DeleteDocument(id);
            return NoContent();
        }

        [HttpGet("{documentId:int}/files")]
        public async Task<IActionResult> GetFilesByDocumentId(int documentId)
        {
            _logger.LogInformation("HTTP GET /api/Document/{DocumentId}/files empfangen.", documentId);
            // Wirft DocumentNotFoundException -> Middleware übersetzt zu 404
            var files = await _documentService.GetFilesByDocumentId(documentId);
            return Ok(files);
        }

        [HttpPut("{id:int}")]
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

        [HttpGet("{id:int}/download")]
        public async Task<IActionResult> DownloadFile(int id)
        {
            _logger.LogInformation("HTTP GET /api/Document/{Id}/download empfangen.", id);

            var downloadDto = await _documentService.DownloadDocument(id);

            return File(downloadDto.FileStream, downloadDto.ContentType, downloadDto.FileName);
        }
    }
}