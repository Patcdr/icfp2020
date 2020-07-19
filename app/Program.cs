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

            Sender sender = new Sender(serverUrl, key);

            // new GameInteractStrategy(sender, new Number(0)).Run(); return -2;

            BaseInteractStrategy strategy;
            if (args.Length == 0)
            {
                // Rumble mode with explicit bots
                strategy = new HeadToHeadStrategy(sender);
            }
            else if (args.Length == 2)
            {
                // Submission mode
                strategy = new GameInteractStrategy(sender, new Number(long.Parse(key)));
            }
            else if (args.Length == 4)
            {
                // Rumble mode with default bots
                strategy = new HeadToHeadStrategy(sender, args[3], args[4]);
            }
            else {
                Console.Error.WriteLine("Invalid arguments");
                return -1;
            }

            strategy.Run();

            // Needed for the rumbletron
            Console.Error.WriteLine($"SCORE: 1000");

            return 0;
        }
    }
}