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
        private static Client CreateClient()
        {
            var apiKey = Environment.GetEnvironmentVariable("GEMINI_API_KEY");
            if (string.IsNullOrWhiteSpace(apiKey))
            {
                throw new InvalidOperationException("Die Umgebungsvariable GEMINI_API_KEY ist nicht gesetzt.");
            }

            return new Client(apiKey: apiKey);
        }

        public async static Task<string> summarizeText(string text)
        {
            Client client = CreateClient();
            var response = await client.Models.GenerateContentAsync(
                model: "gemini-2.5-flash", contents: "The following text is from a document. Summarize this Text in up to 4 sentences. Only answer with the summary: " + text
            );
            return response.Candidates?[0].Content?.Parts?[0].Text ?? "";
        }
        public async static Task<string> categorizeText(string text)
        {
            Client client = CreateClient();
            var response = await client.Models.GenerateContentAsync(
                model: "gemini-2.5-flash", contents: "The following text is from a document. Give this document 1 category. Only answer with this 1 category: " + text
            );
            return response.Candidates?[0].Content?.Parts?[0].Text ?? "";
        }
    }
}
