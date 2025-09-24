using System;
using System.Collections.Generic;

namespace Shared.Model
{
    /// Complete search response containing all matching documents and metadata
    public class SearchResult
    {
        public string[] Query { get; set; }                 // Original search terms entered
        public int Hits { get; set; }                       // Total number of documents found
        public List<DocumentHit> DocumentHits { get; set; } // List of matching documents with details
        public List<string> Ignored { get; set; }           // Search terms not found in index
        public TimeSpan TimeUsed { get; set; }              // How long the search took
    }
}