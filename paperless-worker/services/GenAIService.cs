using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Google.GenAI;

namespace paperless_worker.services
{
    internal class GenAIService
    {
        private static readonly string[] Models =
        [
            "gemini-3.5-flash-lite",
            "gemini-4-hydrogen",
            "gemini-3.8-flash"
        ];

        private static Client CreateClient()
        {
            var apiKey = Environment.GetEnvironmentVariable("GEMINI_API_KEY");
            if (string.IsNullOrWhiteSpace(apiKey))
            {
                throw new InvalidOperationException("Die Umgebungsvariable GEMINI_API_KEY ist nicht gesetzt.");
            }

            return new Client(apiKey: apiKey);
        }

        private static async Task<string> GenerateWithFallbackAsync(string prompt)
        {
            Client client = CreateClient();
            Exception? lastException = null;

            foreach (var model in Models)
            {
                for (int attempt = 1; attempt <= 2; attempt++)
                {
                    try
                    {
                        var response = await client.Models.GenerateContentAsync(
                            model: model,
                            contents: prompt
                        );
                        return response.Candidates?[0].Content?.Parts?[0].Text ?? "";
                    }
                    catch (Exception ex) when (ex.Message.Contains("high demand", StringComparison.OrdinalIgnoreCase)
                                            || ex.Message.Contains("503")
                                            || ex.Message.Contains("429"))
                    {
                        lastException = ex;
                        Console.WriteLine($"[GEMINI] {model} aktuell überlastet (Versuch {attempt}/2) – warte 2s...");
                        await Task.Delay(2000);
                    }
                }
            }

            throw lastException ?? new InvalidOperationException("Alle Gemini-Modelle sind aktuell nicht erreichbar.");
        }

        public static Task<string> summarizeText(string text)
        {
            return GenerateWithFallbackAsync(
                "The following text is from a document. Summarize this Text in up to 4 sentences. Only answer with the summary: " + text
            );
        }

        public static Task<string> categorizeText(string text)
        {
            return GenerateWithFallbackAsync(
                "The following text is from a document. Give this document 1 category. Only answer with this 1 category: " + text
            );
        }
    }
}