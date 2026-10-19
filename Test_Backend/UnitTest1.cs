using Moq;
using Xunit;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using data.Interfaces;
using data.Entities;
using business_layer;
using business_layer.Dtos;
using business_layer.Exceptions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Test_Backend
{
    public class UnitTest1
    {
        // NullLogger verhindert NullReferenceExceptions ohne aufwendiges Logger-Mocking
        private readonly ILogger<DocumentService> _logger = NullLogger<DocumentService>.Instance;

        [Fact]
        public async Task UploadDocument_CallsRepository()
        {
            var repository = new Mock<IDocumentRepository>();
            var service = new DocumentService(repository.Object, _logger);
            repository
               .Setup(r => r.CreateDocument(It.IsAny<Document>()))
               .ReturnsAsync(42);

            int id = await service.UploadDocument(new DocumentUploadDto("Test1.png"));
            Assert.Equal(42, id);

            repository.Verify(
                r => r.CreateFile(It.IsAny<FileVersion>()),
                Times.Once);

            repository.Verify(
                r => r.CreateDocument(It.IsAny<Document>()),
                Times.Once);
        }

        [Fact]
        public async Task UploadDocument_ThrowsValidationException_WhenTitleIsEmpty()
        {
            var repository = new Mock<IDocumentRepository>();
            var service = new DocumentService(repository.Object, _logger);

            // Prüft, ob bei leerem Titel die DocumentValidationException geworfen wird
            await Assert.ThrowsAsync<DocumentValidationException>(() =>
                service.UploadDocument(new DocumentUploadDto("   ")));

            repository.Verify(r => r.CreateDocument(It.IsAny<Document>()), Times.Never);
        }

        [Fact]
        public async Task GetAllDocuments_CallsRepository()
        {
            var documents = new List<Document>
            {
                new Document("test.pdf", null),
                new Document("woow.pdf", null)
            };
            var repository = new Mock<IDocumentRepository>();
            var service = new DocumentService(repository.Object, _logger);
            repository
                .Setup(r => r.GetAllDocuments())
                .ReturnsAsync(documents);

            var results = await service.GetAllDocuments();

            Assert.NotNull(results);
            Assert.Equal(2, results.Count());

            repository.Verify(
                r => r.GetAllDocuments(),
                Times.Once);
        }

        [Fact]
        public async Task GetDocumentById_CallsRepository()
        {
            var id = 1;
            var document = new Document("Test1.pdf", null);
            var repository = new Mock<IDocumentRepository>();

            repository
                .Setup(r => r.GetDocumentById(1))
                .ReturnsAsync(document);

            var service = new DocumentService(repository.Object, _logger);

            var result = await service.GetDocumentById(id);

            Assert.NotNull(result);
            Assert.Equal("Test1.pdf", result.Filename);

            repository.Verify(
                r => r.GetDocumentById(id),
                Times.Once);
        }

        [Fact]
        public async Task GetDocumentById_ThrowsDocumentNotFoundException_WhenIdNotFound()
        {
            var id = 1;
            var repository = new Mock<IDocumentRepository>();

            // ID 1 liefert null
            repository
                .Setup(r => r.GetDocumentById(id))
                .ReturnsAsync((Document?)null);

            var service = new DocumentService(repository.Object, _logger);

            // Prüft schichtenspezifische Exception statt Assert.Null
            var ex = await Assert.ThrowsAsync<DocumentNotFoundException>(() =>
                service.GetDocumentById(id));

            Assert.Equal(id, ex.DocumentId);

            repository.Verify(
                r => r.GetDocumentById(id),
                Times.Once);
        }

        [Fact]
        public async Task DeleteDocument_CallsRepository()
        {
            var document = new Document("Test.pdf", null) { Id = 2 };
            var repository = new Mock<IDocumentRepository>();

            repository
                .Setup(r => r.GetDocumentById(2))
                .ReturnsAsync(document);
            repository
                .Setup(r => r.DeleteDocument(document))
                .ReturnsAsync(true);

            var service = new DocumentService(repository.Object, _logger);

            var result = await service.DeleteDocument(2);

            Assert.True(result);

            repository.Verify(
                r => r.GetDocumentById(2),
                Times.Once);

            repository.Verify(
                r => r.DeleteDocument(document),
                Times.Once);
        }

        [Fact]
        public async Task DeleteDocument_ThrowsDocumentNotFoundException_WhenNotFound()
        {
            var repository = new Mock<IDocumentRepository>();
            repository
                .Setup(r => r.GetDocumentById(99))
                .ReturnsAsync((Document?)null);

            var service = new DocumentService(repository.Object, _logger);

            await Assert.ThrowsAsync<DocumentNotFoundException>(() =>
                service.DeleteDocument(99));

            repository.Verify(r => r.DeleteDocument(It.IsAny<Document>()), Times.Never);
        }
    }
}