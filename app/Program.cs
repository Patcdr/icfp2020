using Core;
using System;
using System.Net.Http;
using System.Net.Mime;
using System.Text;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using static Core.Library;
using System.Runtime.InteropServices;

namespace app
{
    class Program
    {
        public static HttpClient httpClient;
        public static string serverUrl;
        public static string playerKey;

        public static void Brute(IProtocol protocol, Value state)
        {
            var drawing = Drawer.drawing;
            Drawer.drawing = false;

            var start = state.ToString();

            for (int y = -110; y < 110; y += 3)
            {
                for (int x = -110; x < 110; x += 3)
                {
                    var next = Interactor.Interact(protocol, state, new ConsIntermediate2(new Number(x), new Number(y)));
                    if (start != next.NewState.ToString())
                    {
                        Console.WriteLine($"\n    ({x}, {y}) {start} -> {next.NewState}");
                        Drawer.drawing = true;
                        Interactor.Interact(protocol, state, new ConsIntermediate2(new Number(x), new Number(y)));
                        Drawer.drawing = false;
                    }
                }
                Console.Write($"{y} ");
            }

            Console.WriteLine();

            Drawer.drawing = drawing;
        }

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

            IProtocol protocol = new GalaxyProtocol();

            var points = new ConsIntermediate2 [] {
                new ConsIntermediate2(new Number(0), new Number(0)),  // galaxy 0 nil
                new ConsIntermediate2(new Number(0), new Number(0)),  // galaxy 1 nil
                new ConsIntermediate2(new Number(0), new Number(0)),  // galaxy 2 nil
                new ConsIntermediate2(new Number(0), new Number(0)),  // galaxy 3 nil
                new ConsIntermediate2(new Number(0), new Number(0)),  // multidraw
                new ConsIntermediate2(new Number(0), new Number(0)),  // nil ? ? ?
                new ConsIntermediate2(new Number(0), new Number(0)),  // cross @ 0,0
                new ConsIntermediate2(new Number(0), new Number(0)),  // cross @ 8,4
            };

            Drawer.drawing = true;

            Interactor.Result result = null;
            for (int i = 0; i < points.Length; i++)
            {
                var next = result == null ? Nil : result.NewState;

                var point = points[i];
                Console.WriteLine(point.ToString());

                result = Interactor.Interact(protocol, next, point);
            }

            long last_x = -10000;
            long last_y = -100000;
            while(true)
            {
                Dictionary<long, int> y_count = new Dictionary<long, int>();
                Dictionary<long, int> x_count = new Dictionary<long, int>();

                foreach (Value point_list in UtilityFunctions.ListAsEnumerable(result.RawData, null))
                {
                    foreach (Value point in UtilityFunctions.ListAsEnumerable(point_list, null))
                    {
                        var x = point.Invoke(Library.TrueVal, null).AsNumber();
                        var y = point.Invoke(Library.FalseVal, null).AsNumber();
                        if (!y_count.ContainsKey(y)) y_count[y] = 0;
                        if (!x_count.ContainsKey(x)) x_count[x] = 0;
                        y_count[y]++;
                        x_count[x]++;
                    }
                }

                var max_y = y_count.OrderByDescending(i => i.Value).First().Key;
                var max_x = x_count.OrderByDescending(i => i.Value).First().Key;
                Console.WriteLine($"{max_x}, {max_y} {result.Flag}");
                if (max_x == last_x && max_y == last_y) {
                    Drawer.drawing = true;
                    Brute(protocol, result.NewState);
                }
                result = Interactor.Interact(protocol, result.NewState, new ConsIntermediate2(new Number(max_x), new Number(max_y)));
                if (max_x == last_x && max_y == last_y) break;
                last_x = max_x;
                last_y = max_y;
            }

            result = Interactor.Interact(protocol, result.NewState, new ConsIntermediate2(new Number(0), new Number(0)));
            result = Interactor.Interact(protocol, result.NewState, new ConsIntermediate2(new Number(0), new Number(0)));
            result = Interactor.Interact(protocol, result.NewState, new ConsIntermediate2(new Number(0), new Number(0)));
            result = Interactor.Interact(protocol, result.NewState, new ConsIntermediate2(new Number(0), new Number(0)));
            result = Interactor.Interact(protocol, result.NewState, new ConsIntermediate2(new Number(0), new Number(0)));
            result = Interactor.Interact(protocol, result.NewState, new ConsIntermediate2(new Number(0), new Number(0)));
            result = Interactor.Interact(protocol, result.NewState, new ConsIntermediate2(new Number(0), new Number(0)));
            result = Interactor.Interact(protocol, result.NewState, new ConsIntermediate2(new Number(0), new Number(0)));
            result = Interactor.Interact(protocol, result.NewState, new ConsIntermediate2(new Number(0), new Number(0)));

            // Needed for the rumbletron
            Console.Error.WriteLine($"SCORE: 1000");

            return 0;
        }

        public static string Send(Value statement) {
            Console.WriteLine($"Sending: {statement}");

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

    }
}