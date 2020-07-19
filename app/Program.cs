using Core;
using System;
using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
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
                var game = new DoubleRunner(
                    new Sender(serverUrl, key),
                    new DontDieRunner(new Sender(serverUrl, key)),
                    new PatRunner(new Sender(serverUrl, key))
                ).Start();
                Visualize(game);
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

        public static string Visualize(GameState state)
        {
            var guid = Guid.NewGuid().ToString();
            var client = new AmazonS3Client("AKIAV3HLA4UAHMJV4GGM", "zyuUE/gFaGJs2ovAida+D0EKMrycI8TZew8A3CTe", Amazon.RegionEndpoint.USWest2);

            Console.WriteLine($"Uploading for visualization {guid}");
            Console.WriteLine(GameLog.Write(state));

            client.PutObjectAsync(new PutObjectRequest
            {
                BucketName = "logs.rumbletoon.com",
                Key = $"games/{guid}",
                ContentBody = GameLog.Write(state)
            }).Wait();

            client.PutObjectAsync(new PutObjectRequest
            {
                BucketName = "logs.rumbletoon.com",
                Key = $"games/{guid}.json",
                ContentBody = $@"[
                    ""attacker"": [
                        ""playerKey"": ""1001"",
                        ""submissionId"": 0,
                        ""team"": [
                            ""customData"": [
                                ""country"": ""PRK""
                            ],
                            ""teamId"": ""4ce2a471-c310-4dae-8fb0-ef006a1f4e02"",
                            ""teamName"": ""PatRunner""
                        ],
                        ""timeout"": false
                    ],
                    ""createdAt"": ""2020-07-19T20:40:44.81364+03:00"",
                    ""defender"": [
                        ""playerKey"": ""5005"",
                        ""submissionId"": 0,
                        ""team"": [
                            ""customData"": [
                                ""country"": ""PRK""
                            ],
                            ""teamId"": ""4ce2a471-c310-4dae-8fb0-ef006a1f4e02"",
                            ""teamName"": ""PatRunner""
                        ],
                        ""timeout"": false
                    ],
                    ""finishedAt"": ""2020-07-19T20:41:54.297563+03:00"",
                    ""gameId"": ""{guid}"",
                    ""ticks"": {state.CurrentTurn},
                    ""tournamentId"": 0,
                    ""tournamentRoundId"": 0,
                    ""winner"": ""Attacker""
                ]".Replace('[', '{').Replace(']', '}')
            }).Wait();

            Console.WriteLine($"Visualize at: https://allanca.github.io/#/visualize?game={guid}");

            return guid;
        }
    }
}