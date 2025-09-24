using System;
using System.Collections.Generic;

namespace Shared.Model
{
    /// Represents a search result for a single document with hit statistics
    public class DocumentHit
    {
        public BEDocument Document { get; set; }    // The document that matched
        public int NoOfHits { get; set; }           // Number of search term matches found
        public List<string> Missing { get; set; }   // Search terms not found in this document
    }
}
