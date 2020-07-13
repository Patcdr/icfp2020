using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace app
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var serverUrl = new Uri(args[0], UriKind.Absolute);
            var playerKey = args[1];

            Console.Error.WriteLine($"ServerUrl: {serverUrl}; PlayerKey: {playerKey}");

            using var httpClient = new HttpClient {BaseAddress = serverUrl};
            using var responseMessage = await httpClient.GetAsync($"?playerKey={playerKey}");
            responseMessage.EnsureSuccessStatusCode();

            Console.Error.WriteLine($"SCORE: 1000");
        }
    }
}