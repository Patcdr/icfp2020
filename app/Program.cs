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

        public static int Main(string[] args)
        {
            // Default to the test server
            serverUrl = "https://icfpc2020-api.testkontur.ru";
            playerKey = "463bf8217ff3469189e1d9d15f8a29ce";

            if (args.Length == 2)
            {
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

            // Hacky hack!
            //EvaluateGalaxy();

            IProtocol protocol = new GalaxyProtocol();
            //IProtocol protocol = new StatelessDrawProtocol();
            //IProtocol protocol = new StatefulDrawProtocol();

            Interactor.Result result = await Interactor.Interact(protocol);

            for (int y = 0; y < 13; y++)
            {
                for (int x = 0; x < 17; x++)
                {
                    Console.WriteLine($"({x}, {y})");
                    result = await Interactor.Interact(protocol, result.NewState, new ConsIntermediate2(new Number(x), new Number(y)));
                }
            }
            
            //Interactor.Result result = await Interactor.Interact(protocol);
            //result = await Interactor.Interact(protocol, result.NewState, new ConsIntermediate2(new Number(-1), new Number(-3)));
            //result = await Interactor.Interact(protocol, result.NewState, new ConsIntermediate2(new Number(-1), new Number(-3)));
            //result = await Interactor.Interact(protocol, result.NewState, new ConsIntermediate2(new Number(3), new Number(2)));
            //result = await Interactor.Interact(protocol, result.NewState, new ConsIntermediate2(new Number(4), new Number(0)));
            return 0;

            //var content = await Send(new ConsIntermediate2(new Number(1), Library.Nil));

            // Needed for the rumbletron
            Console.Error.WriteLine($"SCORE: 1000");

            return 0;
        }

        public static string Send(Value statement) {
            var signal = NumberFunctions.Mod(statement, null);
            var requestContent = new StringContent(signal, Encoding.UTF8, MediaTypeNames.Text.Plain);
            using var response = httpClient.PostAsync($"/aliens/send?apiKey={playerKey}", requestContent).Result;
            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"Unexpected server response: {response}");
                return "";
            }

            var content = response.Content.ReadAsStringAsync().Result;
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
            Dictionary<string, Node> env = Parser.Parse(lines.ToList());
            Value galaxy = env["almost"].Evaluate(env);
            Value evalGal = UtilityFunctions.EvaluateFully(galaxy, env);
            return evalGal;
        }
    }
}