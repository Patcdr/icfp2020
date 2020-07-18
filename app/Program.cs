using System;

namespace app
{
    class Program
    {
        public static int Main(string[] args)
        {
            // Default to the test server
            string serverUrl = "https://icfpc2020-api.testkontur.ru";
            string apiKey = "463bf8217ff3469189e1d9d15f8a29ce";

            if (args.Length == 2)
            {
                serverUrl = args[0];
                apiKey = args[1];
            }

            Sender sender = new Sender(serverUrl, apiKey);
            Interactor interactor = new Interactor(sender);
            BaseInteractStrategy interactStrategy = new HeadToHeadStrategy(interactor);

            interactStrategy.Execute();

            // Needed for the rumbletron
            Console.Error.WriteLine($"SCORE: 1000");

            return 0;
        }
    }
}