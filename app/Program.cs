using Core;
using System;
using static Core.Library;

namespace app
{
    class Program
    {
        public static int Main(string[] args)
        {
            // Default to the test server
            string serverUrl = "https://icfpc2020-api.testkontur.ru";
            string key = "463bf8217ff3469189e1d9d15f8a29ce";

            if (args.Length >= 2)
            {
                serverUrl = args[0];
                key = args[1];
            }

            Console.Error.WriteLine($"Running against {serverUrl} as {key}. {args.Length}");

            if (args.Length == 0)
            {
                // Rumble mode with explicit bots
                new DoubleRunner(
                    new Sender(serverUrl, key),
                    new PatRunner(new Sender(serverUrl, key)),
                    new PatRunner(new Sender(serverUrl, key))
                ).Start();
            }
            else if (args.Length == 2)
            {
                // Submission mode
                new DontDieRunner(new Sender(serverUrl, key), long.Parse(key)).Start();
            }
            else {
                Console.Error.WriteLine("Invalid arguments");
                return -1;
            }

            // Needed for the rumbletron
            Console.Error.WriteLine($"SCORE: 1000");

            return 0;
        }
    }
}