using Moq;
using System.Timers;
using data;
using data.Interfaces;
using business_layer;

namespace Test_Backend
{
    public class UnitTest1
    {
        [Fact]
        public async Task AddDocument_CallsRepository()
        {
            var repository = new Mock<IDocumentRepository>();
            var service = new DocumentService(repository.Object);

        }
    }
}
