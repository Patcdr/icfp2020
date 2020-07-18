using Core;
using System;
using System.Net.Http;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using static Core.Library;

namespace app
{
    class Program
    {
        public static HttpClient httpClient;
        public static string serverUrl;
        public static string playerKey;

        public static async Task<int> Main(string[] args)
        {
            // Hacky hack!
            //EvaluateGalaxy();

            // Default to the test server
            serverUrl = "https://icfpc2020-api.testkontur.ru";
            playerKey = "463bf8217ff3469189e1d9d15f8a29ce";

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

            httpClient = new HttpClient { BaseAddress = serverUri };

            var content = await Send(new ConsIntermediate2(new Number(1), Library.Nil));

            // Needed for the rumbletron
            Console.Error.WriteLine($"SCORE: 1000");

            return 0;
        }

        public static async Task<string> Send(Value statement) {
            var signal = NumberFunctions.Mod(statement, null);
            var requestContent = new StringContent(signal, Encoding.UTF8, MediaTypeNames.Text.Plain);
            using var response = await httpClient.PostAsync($"/aliens/send?apiKey={playerKey}", requestContent);
            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"Unexpected server response: {response}");
                return "";
            }

            var content = await response.Content.ReadAsStringAsync();
            var answer = NumberFunctions.Dem(content);
            Console.WriteLine($"Server response: {answer}");

            return content;
        }

        // Hack: eventually this will want to move somewhere else!
        public static Value EvaluateGalaxy()
        {
            // Hack: load up Galaxy.txt
            string[] lines = File.ReadAllLines(@"..\..\..\..\galaxy.txt");
            //List<string> testLines = new List<string> { ":example = ap ap ap s isnil :example nil" };
            Dictionary<string, Node> environment = Parser.Parse(lines.ToList());
            Value stuff = environment["almost"].Evaluate(environment);
            var moreStuff = UtilityFunctions.EvaluatePointList(
                stuff.Invoke(FalseVal, environment).Invoke(TrueVal, environment), environment);
            return stuff;
        }
    }
}