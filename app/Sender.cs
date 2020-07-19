using Core;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Mime;
using System.Text;

namespace app
{
    public class Sender
    {
        private readonly string serverUrl;
        private readonly string playerKey;
        private readonly HttpClient httpClient;

        public Sender(string serverUrl, string playerKey)
        {
            this.serverUrl = serverUrl;
            this.playerKey = playerKey;

            //Console.WriteLine($"ServerUrl: {serverUrl}; PlayerKey: {playerKey}");

            if (!Uri.TryCreate(serverUrl, UriKind.Absolute, out var serverUri))
            {
                throw new ArgumentException("Failed to parse serverUrl: " + serverUrl);
            }

            httpClient = new HttpClient { BaseAddress = serverUri };
            httpClient.Timeout = TimeSpan.FromDays(1);
        }

        public Value Send(Value statement, Value player=null)
        {
            // Console.WriteLine($"Sending({player}): {statement}");

            var signal = NumberFunctions.Mod(statement, null);
            var requestContent = new StringContent(signal, Encoding.UTF8, MediaTypeNames.Text.Plain);

            using var response = httpClient.PostAsync($"/aliens/send?apiKey={playerKey}", requestContent).Result;
            if (!response.IsSuccessStatusCode)
            {
                throw new InvalidOperationException($"Unexpected server response: {response}");
            }

            var content = response.Content.ReadAsStringAsync().Result;
            var answer = NumberFunctions.Dem(content);

            // Console.WriteLine($"Received({player}): {answer}");

            return answer;
        }

        public Value Send(Value[] statement, Value player=null)
        {
            return Send(UtilityFunctions.MakeList(statement), player);
        }
    }
}
