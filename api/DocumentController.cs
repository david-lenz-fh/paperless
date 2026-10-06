using business_layer.Dtos;
using business_layer.Interfaces;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace api
{
    [Route("api/[controller]")]
    [ApiController]
    [EnableCors("AllowAll")]
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
        public async Task<ActionResult<List<DocumentDto>>> GetAllDocuments()
        {
            var documents = await _documentService.GetAllDocuments();
            return Ok(documents);
        }
        
        [HttpGet("{id}")]
        public async Task<ActionResult<DocumentDto>> GetDocumentById(int id)
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
            bool isDeleted = await _documentService.DeleteDocument(id);

            if (!isDeleted)
            {
                return NotFound($"Dokument mit der ID {id} konnte nicht gefunden werden.");
            }

            return NoContent();
        }
        
        [HttpGet("{documentId}/files")]
        public async Task<IActionResult> GetFilesByDocumentId(int documentId)
        {
            var document = await _documentService.GetDocumentById(documentId);
            if (document == null)
            {
                return NotFound($"Dokument mit der ID {documentId} wurde nicht gefunden.");
            }

            var files = await _documentService.GetFilesByDocumentId(documentId);

            return Ok(files);
        }
        
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateDocument(int id, [FromBody] DocumentUpdateDto documentToUpdate)
        {
            if (documentToUpdate == null || string.IsNullOrWhiteSpace(documentToUpdate.Title))
            {
                return BadRequest("Ungültige Daten oder Titel ist leer.");
            }

            // id is correct id from url
            documentToUpdate.Id = id;

            try
            {
                int fileVersionId = await _documentService.UpdateDocument(documentToUpdate);
                return Ok(new { FileVersionId = fileVersionId, Message = "Dokument erfolgreich aktualisiert." });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }
        
        /**
        [HttpGet("{id}/download")]
        public async Task<IActionResult> DownloadFile(Guid id)
        {
            return Ok();
        }
        **/
    }
}
