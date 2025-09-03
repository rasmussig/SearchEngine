using System;
using System.Text.Json.Serialization;

namespace Shared.Model
{
    public class BEDocument
    {
        [JsonPropertyName("mId")]
        public int mId { get; set; }

        [JsonPropertyName("mUrl")]
        public String mUrl { get; set; }

        [JsonPropertyName("mIdxTime")]
        public String mIdxTime { get; set; }

        [JsonPropertyName("mCreationTime")]
        public String mCreationTime { get; set; }
    }
}
