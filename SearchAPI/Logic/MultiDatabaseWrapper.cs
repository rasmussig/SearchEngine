using System;
using System.Collections.Generic;
using System.Linq;
using Shared.Model;

namespace SearchAPI
{
    /// <summary>
    /// Y-scaling: Queries multiple database shards and merges results.
    /// Each shard contains a subset of documents (200k each of 600k total).
    /// </summary>
    public class MultiDatabaseWrapper : IDatabase
    {
        private readonly List<IDatabase> _databases;
        
        public MultiDatabaseWrapper(List<IDatabase> databases)
        {
            _databases = databases ?? throw new ArgumentNullException(nameof(databases));
            if (_databases.Count == 0)
                throw new ArgumentException("At least one database required");
        }

        public List<int> GetWordIds(string[] query, out List<string> outIgnored, bool caseSensitive = false)
        {
            var allWordIds = new List<int>();
            var ignoredSet = new HashSet<string>(query);

            // Try each database to find word IDs
            foreach (var word in query)
            {
                foreach (var db in _databases)
                {
                    var wordIds = db.GetWordIds(new[] { word }, out _, caseSensitive);
                    if (wordIds.Count > 0)
                    {
                        allWordIds.AddRange(wordIds);
                        ignoredSet.Remove(word);
                        break;
                    }
                }
            }

            outIgnored = ignoredSet.ToList();
            return allWordIds.Distinct().ToList();
        }

        public List<KeyValuePair<int, int>> GetDocuments(List<int> wordIds)
        {
            Console.WriteLine($"[Y-Scaling] Querying {_databases.Count} shards...");
            var allDocs = new Dictionary<int, int>(); // compositeId -> hitCount
            
            int shardIndex = 0;
            foreach (var db in _databases)
            {
                var docs = db.GetDocuments(wordIds);
                Console.WriteLine($"[Y-Scaling] Shard {shardIndex + 1}: Found {docs.Count} documents");
                foreach (var doc in docs)
                {
                    // Create composite ID: shardIndex * 1000000 + originalDocId
                    int compositeId = shardIndex * 1000000 + doc.Key;
                    allDocs[compositeId] = doc.Value;
                }
                shardIndex++;
            }

            Console.WriteLine($"[Y-Scaling] Total merged: {allDocs.Count} unique documents");
            
            // Sort by hit count descending
            return allDocs
                .OrderByDescending(kvp => kvp.Value)
                .Select(kvp => new KeyValuePair<int, int>(kvp.Key, kvp.Value))
                .ToList();
        }

        public List<BEDocument> GetDocDetails(List<int> compositeDocIds)
        {
            var results = new List<BEDocument>();
            
            // Group by shard
            var byDatabase = compositeDocIds
                .GroupBy(id => id / 1000000)
                .ToDictionary(g => g.Key, g => g.Select(id => id % 1000000).ToList());

            foreach (var kvp in byDatabase)
            {
                int shardIndex = kvp.Key;
                var originalIds = kvp.Value;
                
                if (shardIndex < _databases.Count)
                {
                    var docs = _databases[shardIndex].GetDocDetails(originalIds);
                    foreach (var doc in docs)
                    {
                        doc.mId = shardIndex * 1000000 + doc.mId;
                        results.Add(doc);
                    }
                }
            }

            return results;
        }

        public List<int> getMissing(int compositeDocId, List<int> wordIds)
        {
            int shardIndex = compositeDocId / 1000000;
            int originalId = compositeDocId % 1000000;
            
            if (shardIndex < _databases.Count)
            {
                return _databases[shardIndex].getMissing(originalId, wordIds);
            }
            
            return new List<int>();
        }

        public List<string> WordsFromIds(List<int> wordIds)
        {
            var words = new HashSet<string>();
            
            foreach (var db in _databases)
            {
                var dbWords = db.WordsFromIds(wordIds);
                foreach (var word in dbWords)
                {
                    words.Add(word);
                }
            }
            
            return words.ToList();
        }
    }
}
