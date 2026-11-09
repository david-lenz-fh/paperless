using Moq;
using Xunit;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using data.Interfaces;
using data.Entities;
using business_layer;
using business_layer.Dtos;
using business_layer.Exceptions;
using business_layer.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Test_Backend
{
    public class UnitTest1
    {
        private readonly Mock<IDocumentRepository> _repoMock = new();
        private readonly Mock<IFileStorageService> _storageMock = new();
        private readonly ILogger<DocumentService> _logger = NullLogger<DocumentService>.Instance;

        private DocumentService CreateService() =>
            new DocumentService(_repoMock.Object, _storageMock.Object, _logger);

        [Fact]
        public async Task UploadDocument_UploadsToMinioAndSavesToDatabase()
        {
            // Arrange
            var fakePdfBytes = Encoding.UTF8.GetBytes("%PDF-1.4 fake pdf content");
            using var memoryStream = new MemoryStream(fakePdfBytes);

            var uploadDto = new DocumentUploadDto(
                title: "Rechnung 2026",
                fileStream: memoryStream,
                contentType: "application/pdf",
                fileSize: fakePdfBytes.Length,
                originalFileName: "rechnung.pdf"
            );

            _repoMock
                .Setup(r => r.CreateDocument(It.IsAny<Document>()))
                .ReturnsAsync(42);

            _storageMock
                .Setup(s => s.UploadFileAsync(
                    "doc_42_v1.pdf",
                    It.IsAny<Stream>(),
                    fakePdfBytes.Length,
                    "application/pdf"))
                .ReturnsAsync("documents/doc_42_v1.pdf");

            var service = CreateService();

            // Act
            int id = await service.UploadDocument(uploadDto);

            // Assert
            Assert.Equal(42, id);

            // Prüft, ob MinIO wirklich aufgerufen wurde
            _storageMock.Verify(
                s => s.UploadFileAsync("doc_42_v1.pdf", It.IsAny<Stream>(), fakePdfBytes.Length, "application/pdf"),
                Times.Once);

            // Prüft, ob die FileVersion mit echtem MinIO-Pfad und berechnetem SHA256-Hash in der DB landet
            _repoMock.Verify(
                r => r.CreateFile(It.Is<FileVersion>(f =>
                    f.DocumentId == 42 &&
                    f.StoragePath == "documents/doc_42_v1.pdf" &&
                    f.FileSizeInBytes == fakePdfBytes.Length &&
                    !string.IsNullOrEmpty(f.FileHash))),
                Times.Once);
        }

        [Fact]
        public async Task UploadDocument_ThrowsValidationException_WhenTitleIsEmpty()
        {
            using var stream = new MemoryStream(Encoding.UTF8.GetBytes("data"));
            var uploadDto = new DocumentUploadDto("   ", stream, "application/pdf", 4, "test.pdf");
            var service = CreateService();

            await Assert.ThrowsAsync<DocumentValidationException>(() =>
                service.UploadDocument(uploadDto));

            _storageMock.Verify(s => s.UploadFileAsync(It.IsAny<string>(), It.IsAny<Stream>(), It.IsAny<long>(), It.IsAny<string>()), Times.Never);
            _repoMock.Verify(r => r.CreateDocument(It.IsAny<Document>()), Times.Never);
        }

        [Fact]
        public async Task GetAllDocuments_CallsRepository()
        {
            var documents = new List<Document>
            {
                new Document("test.pdf", null),
                new Document("woow.pdf", null)
            };
            _repoMock.Setup(r => r.GetAllDocuments()).ReturnsAsync(documents);

            var service = CreateService();
            var results = await service.GetAllDocuments();

            Assert.NotNull(results);
            Assert.Equal(2, results.Count());
            _repoMock.Verify(r => r.GetAllDocuments(), Times.Once);
        }

        [Fact]
        public async Task GetDocumentById_CallsRepository()
        {
            var document = new Document("Test1.pdf", null);
            _repoMock.Setup(r => r.GetDocumentById(1)).ReturnsAsync(document);

            var service = CreateService();
            var result = await service.GetDocumentById(1);

            Assert.NotNull(result);
            Assert.Equal("Test1.pdf", result.Filename);
            _repoMock.Verify(r => r.GetDocumentById(1), Times.Once);
        }

        [Fact]
        public async Task GetDocumentById_ThrowsDocumentNotFoundException_WhenIdNotFound()
        {
            _repoMock.Setup(r => r.GetDocumentById(1)).ReturnsAsync((Document?)null);

            var service = CreateService();

            var ex = await Assert.ThrowsAsync<DocumentNotFoundException>(() =>
                service.GetDocumentById(1));

            Assert.Equal(1, ex.DocumentId);
        }

        [Fact]
        public async Task DeleteDocument_DeletesFromMinioAndRepository()
        {
            var document = new Document("Test.pdf", null) { Id = 2 };
            var files = new List<FileVersion>
            {
                new FileVersion(2, "application/pdf", 100, "hash", "documents/doc_2_v1.pdf", "summary", 1)
            };

            _repoMock.Setup(r => r.GetDocumentById(2)).ReturnsAsync(document);
            _repoMock.Setup(r => r.GetFilesByDocumentId(2)).ReturnsAsync(files);
            _repoMock.Setup(r => r.DeleteDocument(document)).ReturnsAsync(true);

            var service = CreateService();
            var result = await service.DeleteDocument(2);

            Assert.True(result);
            _storageMock.Verify(s => s.DeleteFileAsync("doc_2_v1.pdf"), Times.Once);
            _repoMock.Verify(r => r.DeleteDocument(document), Times.Once);
        }

        [Fact]
        public async Task DeleteDocument_ThrowsDocumentNotFoundException_WhenNotFound()
        {
            _repoMock.Setup(r => r.GetDocumentById(99)).ReturnsAsync((Document?)null);

            var service = CreateService();

            await Assert.ThrowsAsync<DocumentNotFoundException>(() =>
                service.DeleteDocument(99));

            _repoMock.Verify(r => r.DeleteDocument(It.IsAny<Document>()), Times.Never);
        }
    }
}