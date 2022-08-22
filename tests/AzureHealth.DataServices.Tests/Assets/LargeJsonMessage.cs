﻿using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace AzureHealth.DataServices.Tests.Assets
{
    [Serializable]
    [JsonObject]
    public class LargeJsonMessage
    {
        private const string Chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

        public LargeJsonMessage()
        {
            values = new List<string>();
            Fields = new List<KeyValuePair<string, string>>();
        }



        private readonly List<string> values;

        [JsonProperty("fields")]
        public List<KeyValuePair<string, string>> Fields { get; set; }


        public void Load(int fields, int totalSizeBytes)
        {
            int length = totalSizeBytes / fields;

            var random = new Random();

            for (int i = 0; i < fields; i++)
            {
                var randomString = new string(Enumerable.Repeat(Chars, length)
                                                        .Select(s => s[random.Next(s.Length)]).ToArray());
                values.Add(randomString);
            }

            for (int i = 0; i < values.Count; i++)
            {
                Fields.Add(new KeyValuePair<string, string>($"field{i}", values[i]));
            }

        }
    }
}
