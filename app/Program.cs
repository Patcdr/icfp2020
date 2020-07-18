﻿using Core;
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
            // IProtocol protocol = new StatefulDrawProtocol();

            var points = new ConsIntermediate2 [] {
                new ConsIntermediate2(new Number(0), new Number(0)),  // galaxy 0 nil
                new ConsIntermediate2(new Number(0), new Number(0)),  // galaxy 1 nil
                new ConsIntermediate2(new Number(0), new Number(0)),  // galaxy 2 nil
                new ConsIntermediate2(new Number(0), new Number(0)),  // galaxy 3 nil
                new ConsIntermediate2(new Number(0), new Number(0)),  // multidraw
                new ConsIntermediate2(new Number(0), new Number(0)),  // nil ? ? ?
                new ConsIntermediate2(new Number(0), new Number(0)),  // cross @ 0,0
                new ConsIntermediate2(new Number(0), new Number(0)),  // cross @ 8,4
                /*new ConsIntermediate2(new Number(8), new Number(4)),  // cross @ 2,-8
                new ConsIntermediate2(new Number(2), new Number(-8)),  // decross @ 3,6
                new ConsIntermediate2(new Number(3), new Number(6)),  // de cross @ 0, -14
                new ConsIntermediate2(new Number(0), new Number(-14)), // de cross @ -4, 10
                new ConsIntermediate2(new Number(-4), new Number(10)), // de cross @ 9, -3
                new ConsIntermediate2(new Number(9), new Number(-3)),*/ // de cross @ -4, 10
            };
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
                if (max_x == last_x && max_y == last_y) Drawer.drawing = true;
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

            return 0;
            for (int y = -10; y < 10; y++)
            {
                for (int x = -10; x < 10; x++)
                {
                    Console.WriteLine($"({x}, {y})");
                    Interactor.Interact(protocol, result.NewState, new ConsIntermediate2(new Number(0), new Number(0)));
                }
            }

            //Interactor.Result result = Interactor.Interact(protocol);
            //result = Interactor.Interact(protocol, result.NewState, new ConsIntermediate2(new Number(-1), new Number(-3)));
            //result = Interactor.Interact(protocol, result.NewState, new ConsIntermediate2(new Number(-1), new Number(-3)));
            //result = Interactor.Interact(protocol, result.NewState, new ConsIntermediate2(new Number(3), new Number(2)));
            //result = Interactor.Interact(protocol, result.NewState, new ConsIntermediate2(new Number(4), new Number(0)));
            return 0;

            //var content = Send(new ConsIntermediate2(new Number(1), Library.Nil));

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