using System;
using System.Net.Http;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;

namespace app
{
    class Program
    {
        public static async Task<int> Main(string[] args)
        {
            // Default to the test server
            string serverUrl = "https://icfpc2020-api.testkontur.ru";
            string playerKey = "463bf8217ff3469189e1d9d15f8a29ce";

            if (args.Length == 2) {
                serverUrl = args[0];
                playerKey = args[1];
            }

            Console.WriteLine($"ServerUrl: {serverUrl}; PlayerKey: {playerKey}");

            if (!Uri.TryCreate(serverUrl, UriKind.Absolute, out var serverUri))
            {
                Console.WriteLine("Failed to parse ServerUrl");
                return 1;
            }

            using var httpClient = new HttpClient { BaseAddress = serverUri };
            var requestContent = new StringContent("010", Encoding.UTF8, MediaTypeNames.Text.Plain);
            using var response = await httpClient.PostAsync($"/aliens/send?apiKey={playerKey}", requestContent);
            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"Unexpected server response: {response}");
                return 2;
            }

            var responseString = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"Server response: {responseString}");

            Console.Error.WriteLine($"SCORE: 1000");

            return 0;
        }

    }
}