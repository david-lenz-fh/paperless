using Elastic.Clients.Elasticsearch;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using paperless_worker.Models;
using System;
using System.Threading.Tasks;

namespace paperless_worker.Services
{
    public class ElasticsearchService
    {
        private readonly ElasticsearchClient _client;
        private readonly ILogger<ElasticsearchService> _logger;
        private const string IndexName = "documents";

        public ElasticsearchService(IConfiguration configuration, ILogger<ElasticsearchService> logger)
        {
            _logger = logger;
            var uri = configuration["Elasticsearch:Uri"] ?? "http://localhost:9200";

            var settings = new ElasticsearchClientSettings(new Uri(uri))
                .DefaultIndex(IndexName);

            _client = new ElasticsearchClient(settings);
        }

        public async Task IndexDocumentAsync(int id, string title, string filename, string ocrText)
        {
            _logger.LogInformation("Indexiere Dokument ID {DocumentId} in Elasticsearch...", id);

            // Prüfen, ob der Index existiert, falls nicht, anlegen
            var indexExistsResponse = await _client.Indices.ExistsAsync(IndexName);
            if (!indexExistsResponse.Exists)
            {
                await _client.Indices.CreateAsync(IndexName);
            }

            var doc = new DocumentIndexDocument
            {
                Id = id,
                Title = title,
                Filename = filename,
                Content = ocrText,
                CreatedAt = DateTime.UtcNow
            };

            // Dokument indexieren (ID wird als Elasticsearch-Dokument-ID genutzt)
            var response = await _client.IndexAsync(doc, idx => idx.Index(IndexName).Id(id));

            if (response.IsValidResponse)
            {
                _logger.LogInformation("Dokument ID {Id} erfolgreich in Elasticsearch indexiert.", id);
            }
            else
            {
                _logger.LogError("Fehler beim Indexieren von Dokument ID {Id}: {Error}", id, response.DebugInformation);
            }
        }
    }
}