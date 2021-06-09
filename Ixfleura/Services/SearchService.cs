using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;

namespace Ixfleura.Services
{
    public class SearchService : IxService
    {
        private readonly HttpClient _client;

        public SearchService(ILogger<SearchService> logger, HttpClient client) : base(logger)
        {
            _client = client;
        }

        public async Task<(string question, string answer)> GetTriviaQuestionAsync()
        {
            var response = await _client.GetStringAsync("https://jservice.io/api/random");
            var a = JArray.Parse(response);
            var o = JObject.FromObject(a[0]);

            var question = o["question"];
            var answer = o["answer"];
            var cleanAnswer = StripHtmlTags((string) answer);

            return ((string) question, cleanAnswer.ToLower());
        }

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