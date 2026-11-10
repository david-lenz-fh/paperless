using System;
using System.Threading.Tasks;
using Elastic.Clients.Elasticsearch;
using paperless_worker.Models;

namespace paperless_worker.services
{
    public class ElasticsearchService
    {
        private readonly ElasticsearchClient _client;
        private const string IndexName = "documents";

        public ElasticsearchService(string? uri = null)
        {
            uri ??= Environment.GetEnvironmentVariable("Elasticsearch__Uri")
                    ?? "http://elasticsearch:9200";

            var settings = new ElasticsearchClientSettings(new Uri(uri))
                .DefaultIndex(IndexName);

            _client = new ElasticsearchClient(settings);
        }

        public async Task IndexDocumentAsync(int id, string title, string filename, string ocrText)
        {
            Console.WriteLine($"[ES] Indexiere Dokument ID {id} in Elasticsearch...");

            // Index anlegen, falls noch nicht vorhanden
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

            var response = await _client.IndexAsync(doc, idx => idx.Index(IndexName).Id(id));
            4444
            if (response.IsValidResponse)
            {
                Console.WriteLine($"[ES] Dokument ID {id} erfolgreich in Elasticsearch indexiert.");
            }
            else
            {
                Console.WriteLine($"[ES][ERROR] Fehler beim Indexieren: {response.DebugInformation}");
            }
        }
    }
}