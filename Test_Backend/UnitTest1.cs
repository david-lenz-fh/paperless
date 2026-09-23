using Moq;
using System.Timers;
using data;
using data.Interfaces;
using business_layer;
using business_layer.Dtos;
using data.Entities;
namespace Test_Backend
{
    public class UnitTest1
    {
        [Fact]
        public async Task UploadDocument_CallsRepository()
        {
            var repository = new Mock<IDocumentRepository>();
            var service = new DocumentService(repository.Object);
            repository
               .Setup(r => r.CreateDocument(It.IsAny<Document>()))
               .ReturnsAsync(42);

            int id = await service.UploadDocument(new DocumentUploadDto("Test1.png"));
            Assert.Equal(42, id);
            repository.Verify(
                r => r.CreateFile(It.IsAny<FileVersion> ()),
                Times.Once);

            repository.Verify(
                r => r.CreateDocument(It.IsAny<Document>()),
                Times.Once);
        }

        [Fact]
        public async Task GetAllDocuments_CallsRepository()
        {
            var documents = new List<Document>
            {
                new Document("test.pdf"),
                new Document("woow.pdf")
            };
            var repository = new Mock<IDocumentRepository>();
            var service = new DocumentService(repository.Object);
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

            var document = new Document("Test1.pdf");

            var repository = new Mock<IDocumentRepository>();

            repository
                .Setup(r => r.GetDocumentById(1))
                .ReturnsAsync(document);

            var service = new DocumentService(repository.Object);

            var result = await service.GetDocumentById(id);

            Assert.NotNull(result);

            repository.Verify(
                r => r.GetDocumentById(id),
                Times.Once);
        }
        [Fact]
        public async Task GetDocumentById_CallsRepositoryWithInvalidId()
        {
            var id = 1;

            var document = new Document("Test1.pdf");

            var repository = new Mock<IDocumentRepository>();

            repository
                .Setup(r => r.GetDocumentById(2))
                .ReturnsAsync(document);

            var service = new DocumentService(repository.Object);

            var result = await service.GetDocumentById(id);

            Assert.Null(result);

            repository.Verify(
                r => r.GetDocumentById(id),
                Times.Once);
        }
        [Fact]
        public async Task DeleteDocument_CallsRepository()
        {
            var document = new Document(2, "Test.pdf");

            var repository = new Mock<IDocumentRepository>();

            repository
                .Setup(r => r.GetDocumentById(2))
                .ReturnsAsync(document);
            repository
                .Setup(r => r.DeleteDocument(document))
                .ReturnsAsync(true);

            var service = new DocumentService(repository.Object);

            var result = await service.DeleteDocument(2);

            Assert.True(result);

            repository.Verify(
                r => r.GetDocumentById(2),
                Times.Once);

            repository.Verify(
                r => r.DeleteDocument(document),
                Times.Once);
        }
    }
}
