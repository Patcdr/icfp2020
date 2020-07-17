using System;
using System.Net.Http;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;

namespace app
{
    class Program
    {
        public static async Task<int> xMain(string[] args)
        {
            var serverUrl = args[0];
            var playerKey = args[1];
            Console.WriteLine($"ServerUrl: {serverUrl}; PlayerKey: {playerKey}");

            if (!Uri.TryCreate(serverUrl, UriKind.Absolute, out var serverUri))
            {
                Console.WriteLine("Failed to parse ServerUrl");
                return 1;
            }

            using var httpClient = new HttpClient { BaseAddress = serverUri };
            var requestContent = new StringContent(playerKey, Encoding.UTF8, MediaTypeNames.Text.Plain);
            using var response = await httpClient.PostAsync("", requestContent);
            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"Unexpected server response: {response}");
                return 2;
            }

            var responseString = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"Server response: {responseString}");

            return 0;
        }

    }
}