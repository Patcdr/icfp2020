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
                var runner = new DoubleRunner(
                    new Sender(serverUrl, key),
                    new DeathStarRunner(new Sender(serverUrl, key)),
                    new DontDieRunner(new Sender(serverUrl, key)));

                var game = runner.Join();

                for (long i = game.CurrentTurn; i < game.TotalTurns; i++)
                {
                    //Console.WriteLine(game);
                    if (game.GameStateVal == 2) break;

                    game = runner.Step();
                }

                var summary = runner.Attacker.Summarize();
                Visualize(summary, runner.Attacker, runner.Defender);
            }
            else if (args.Length == 2)
            {
                // Submission mode
                var agent = new DeathStarRunner(new Sender(serverUrl, key), long.Parse(key));
                agent.Join();

                for (long i = agent.State.CurrentTurn; i < agent.State.TotalTurns; i++)
                {
                    // Is the game over?
                    if (agent.IsDone)
                    {
                        break;
                    }

                    agent.Step();
                }
            }
            else {
                Console.Error.WriteLine("Invalid arguments");
                return -1;
            }

            // Needed for the rumbletron
            Console.Error.WriteLine($"SCORE: 1000");

            return 0;
        }

        public static string Visualize(Value summary, BaseRunner attacker, BaseRunner defender)
        {
            var guid = Guid.NewGuid().ToString();
            var client = new AmazonS3Client("AKIAV3HLA4UAHMJV4GGM", "zyuUE/gFaGJs2ovAida+D0EKMrycI8TZew8A3CTe", Amazon.RegionEndpoint.USWest2);

            Console.WriteLine($"Uploading for visualization {guid}");

            var log = UtilityFunctions.PrettyPrint(summary, true).Trim().Trim(',');
            log = log.Replace('(', '[').Replace(')', ']');
            log = log.Replace("\n", "").Replace(", ]", "]").Replace(')', ']').Replace('(','[');
            client.PutObjectAsync(new PutObjectRequest
            {
                BucketName = "logs.rumbletoon.com",
                Key = $"games/{guid}",
                ContentBody = log
            }).Wait();

            var ticks = UtilityFunctions.Addr("cdddar", summary).AsNumber();
            var role = UtilityFunctions.Addr("cddddaaar", summary).AsNumber();
            var status = UtilityFunctions.Addr("cddddaaddar", summary).AsNumber();
            var winner = (
                (role == 0 && status == 3) || (role == 1 && status == 4) ?
                "Attacker" : "Defender"
            );

            client.PutObjectAsync(new PutObjectRequest
            {
                BucketName = "logs.rumbletoon.com",
                Key = $"games/{guid}.json",
                ContentBody = $@"[
                    ""attacker"": [
                        ""playerKey"": ""{attacker.Player.AsNumber()}"",
                        ""submissionId"": 0,
                        ""team"": [
                            ""customData"": [
                                ""country"": ""PRK""
                            ],
                            ""teamId"": ""4ce2a471-c310-4dae-8fb0-ef006a1f4e02"",
                            ""teamName"": ""{attacker.GetType().Name}""
                        ],
                        ""timeout"": false
                    ],
                    ""createdAt"": ""{DateTime.UtcNow.ToString("o")}"",
                    ""defender"": [
                        ""playerKey"": ""{defender.Player.AsNumber()}"",
                        ""submissionId"": 0,
                        ""team"": [
                            ""customData"": [
                                ""country"": ""PRK""
                            ],
                            ""teamId"": ""4ce2a471-c310-4dae-8fb0-ef006a1f4e02"",
                            ""teamName"": ""{defender.GetType().Name}""
                        ],
                        ""timeout"": false
                    ],
                    ""finishedAt"": ""{DateTime.UtcNow.ToString("o")}"",
                    ""gameId"": ""{guid}"",
                    ""ticks"": {ticks},
                    ""tournamentId"": 0,
                    ""tournamentRoundId"": 0,
                    ""winner"": ""{winner}""
                ]".Replace('[', '{').Replace(']', '}')
            }).Wait();

            Console.WriteLine($"Visualize at: https://allanca.github.io/#/visualize?game={guid}");

            return guid;
        }
    }
}