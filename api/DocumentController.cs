using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using business_layer.Interfaces;
using business_layer.Dtos;

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

        [HttpPost]
        public async Task<IActionResult> UploadDocument(DocumentUploadDto fileDto)
        {
            if (string.IsNullOrWhiteSpace(fileDto.Title))
            {
                return BadRequest("No file uploaded.");
            }

            await _documentService.UploadDocument(fileDto);
            return Ok();
        }
        
        [HttpGet]
        public async Task<IActionResult> GetAllDocuments()
        {
            var documents = await _documentService.GetAllDocuments();
            return Ok(documents);
        }
        
        [HttpGet("{id}")]
        public async Task<IActionResult> GetDocumentById(int id)
        {
            var document = await _documentService.GetDocumentById(id);
            if (document == null)
            {
                return NotFound($"Document with ID {id} wnot found.");
            }
            return Ok(document);
        }
        
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDocument(int id)
        {
            // 1. Service-Methode in der BLL aufrufen
            bool isDeleted = await _documentService.DeleteDocument(id);

            // 2. Falls das Dokument nicht gefunden wurde -> 404 Not Found
            if (!isDeleted)
            {
                return NotFound($"Dokument mit der ID {id} konnte nicht gefunden werden.");
            }

            // 3. Erfolgreich gelöscht -> 204 No Content (Standard für HTTP DELETE)
            return NoContent();
        }
        
        [HttpGet("{documentId}/files")]
        public async Task<IActionResult> GetFilesByDocumentId(int documentId)
        {
            // 1. (Optional aber empfohlen) Prüfen, ob das Dokument existiert
            var document = await _documentService.GetDocumentById(documentId);
            if (document == null)
            {
                return NotFound($"Dokument mit der ID {documentId} wurde nicht gefunden.");
            }

            // 2. Dateiversionen aus der BLL abrufen
            var files = await _documentService.GetFilesByDocumentId(documentId);

            // 3. Liste als HTTP 200 OK zurückgeben
            return Ok(files);
        }
        
        /**

        [HttpPut]
        public async Task<IActionResult> EditFile(UploadDocumentRequest document)
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
        **/
    }
}
