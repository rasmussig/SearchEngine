using System;

namespace Shared.Model
{
    /// Represents a single document in the search index with metadata
    public class BEDocument
    {
        public int mId { get; set; }           // Unique document ID in database
        public string mUrl { get; set; }       // Full file path to the document
        public string mIdxTime { get; set; }   // When document was indexed
        public string mCreationTime { get; set; } // When document was created
    }
}
