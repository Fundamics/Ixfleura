using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;

namespace Ixfleura.Services
{
    /// <summary>
    /// A service to assist with accessing APIs and returning data.
    /// </summary>
    public class SearchService : IxfleuraService
    {
        private readonly HttpClient _client;

        public SearchService(ILogger<SearchService> logger, HttpClient client) : base(logger)
        {
            _client = client;
        }

        /// <summary>
        /// Gets a trivia question.
        /// </summary>
        /// <returns>
        /// A tuple consisting of the cleaned answer and question.
        /// </returns>
        public async Task<(string question, string answer)> GetTriviaQuestionAsync()
        {
            var response = await _client.GetStringAsync("https://jservice.io/api/random");
            var a = JArray.Parse(response);
            var o = JObject.FromObject(a[0]);

            var question = (string) o["question"];
            var answer = (string) o["answer"];
            var cleanAnswer = StripHtmlTags(answer).ToLower();

            return (question, cleanAnswer);
        }

        /// <summary>
        /// Strips html tags from the given source.
        /// </summary>
        /// <param name="source">The string to clean tags from.</param>
        /// <returns>The cleaned string.</returns>
        private string StripHtmlTags(string source)
        {
            var array = new char[source.Length];
            var arrayIndex = 0;
            var inside = false;

            foreach (var let in source)
            {
                switch (let)
                {
                    case '<':
                        inside = true;
                        continue;
                    case '>':
                        inside = false;
                        continue;
                }

                if (!inside)
                {
                    array[arrayIndex] = let;
                    arrayIndex++;
                }
            }

            return new string(array, 0, arrayIndex);
        }
    }
}
