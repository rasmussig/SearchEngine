using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Shared.Model
{
    public class DocumentHit
    {
        [JsonConstructor]
        public DocumentHit(BEDocument document, int noOfHits, List<string> missing)
        {
            Document = document;
            NoOfHits = noOfHits;
            Missing = missing;
        }

        [JsonPropertyName("document")]
        public BEDocument Document { get;  }

        [JsonPropertyName("noOfHits")]
        public int NoOfHits { get;  }

        [JsonPropertyName("missing")]
        public List<string> Missing { get;  }
    }
}
