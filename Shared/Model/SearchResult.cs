using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Shared.Model
{
    public class SearchResult
    {
        [JsonConstructor]
        public SearchResult(String[] query, int hits, List<DocumentHit> documentHits, List<string> ignored, TimeSpan timeUsed)
        {
            Query = query;
            Hits = hits;
            DocumentHits = documentHits;
            Ignored = ignored;
            TimeUsed = timeUsed;
        }

        [JsonPropertyName("query")]
        public String[] Query { get; }

        [JsonPropertyName("hits")]
        public int Hits { get; }

        [JsonPropertyName("documentHits")]
        public List<DocumentHit> DocumentHits { get; }

        [JsonPropertyName("ignored")]
        public List<string> Ignored { get; }

        [JsonPropertyName("timeUsed")]
        public TimeSpan TimeUsed { get; }
    }
}