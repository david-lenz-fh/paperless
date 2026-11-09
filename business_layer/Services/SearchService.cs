using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using business_layer.Dtos;
using business_layer.Interfaces;
using Elastic.Clients.Elasticsearch;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace business_layer
{
    public class SearchService : ISearchService
    {
        private readonly ElasticsearchClient _client;
        private readonly ILogger<SearchService> _logger;
        private const string IndexName = "documents";

        public SearchService(IConfiguration configuration, ILogger<SearchService> logger)
        {
            _logger = logger;
            var uri = configuration["Elasticsearch:Uri"] ?? "http://localhost:9200";

            var settings = new ElasticsearchClientSettings(new Uri(uri))
                .DefaultIndex(IndexName);

            _client = new ElasticsearchClient(settings);
        }

        public async Task<IEnumerable<DocumentSearchResultDto>> SearchDocumentsAsync(string searchTerm)
        {
            _logger.LogInformation("Führe Volltextsuche in Elasticsearch aus für Begriff: '{SearchTerm}'", searchTerm);

            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                return Enumerable.Empty<DocumentSearchResultDto>();
            }

            // Elasticsearch Query: Durchsuche das 'content'- und 'title'-Feld nach dem Suchbegriff
            var searchResponse = await _client.SearchAsync<DocumentIndexDocument>(s => s
                .Index(IndexName)
                .Query(q => q
                    .MultiMatch(mm => mm
                        .Query(searchTerm)
                        .Fields(new[] { "content", "title", "filename" })
                    )
                )
            );

            if (!searchResponse.IsValidResponse)
            {
                _logger.LogError("Fehler bei der Elasticsearch-Suche: {Error}", searchResponse.DebugInformation);
                return Enumerable.Empty<DocumentSearchResultDto>();
            }

            // Treffer auf DTO mappen
            var results = searchResponse.Hits.Select(hit => new DocumentSearchResultDto
            {
                Id = hit.Source.Id,
                Title = hit.Source.Title,
                Filename = hit.Source.Filename,
                Content = hit.Source.Content,
                CreatedAt = hit.Source.CreatedAt,
                Score = hit.Score ?? 0.0
            });

            _logger.LogInformation("Suche erfolgreich. {Count} Treffer gefunden.", results.Count());
            return results;
        }
    }

    // Hilfsmodell zum deserialisieren
    internal class DocumentIndexDocument
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Filename { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}