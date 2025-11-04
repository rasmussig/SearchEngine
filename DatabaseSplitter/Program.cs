using System;
using System.IO;
using Microsoft.Data.Sqlite;
using Shared;

namespace DatabaseSplitter
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("=== Database Splitter til Y-Skalering ===\n");
            
            string sourceDb = Paths.DATABASE;
            
            if (!File.Exists(sourceDb))
            {
                Console.WriteLine($"ERROR: Kan ikke finde source database: {sourceDb}");
                Console.WriteLine("Kør indexer først for at oprette databasen!");
                return;
            }
            
            Console.WriteLine($"Source database: {sourceDb}");
            
            // Tjek antal dokumenter
            int totalDocs = GetDocumentCount(sourceDb);
            Console.WriteLine($"Total dokumenter: {totalDocs}");
            
            if (totalDocs == 0)
            {
                Console.WriteLine("ERROR: Databasen indeholder ingen dokumenter!");
                return;
            }
            
            // Spørg hvor mange shards
            Console.Write("\nHvor mange shards vil du oprette? (2-10): ");
            string? input = Console.ReadLine();
            
            if (!int.TryParse(input, out int numShards) || numShards < 2 || numShards > 10)
            {
                Console.WriteLine("Ugyldig input. Bruger 3 shards som standard.");
                numShards = 3;
            }
            
            int docsPerShard = totalDocs / numShards;
            int remainder = totalDocs % numShards;
            
            Console.WriteLine($"\nOpretter {numShards} shards med ~{docsPerShard} dokumenter hver");
            Console.WriteLine($"(Sidste shard får {remainder} ekstra dokumenter)\n");
            
            // Opret shards
            string dataPath = Paths.GetDataPath();
            
            for (int i = 1; i <= numShards; i++)
            {
                int startDocId = (i - 1) * docsPerShard + 1;
                int endDocId = i * docsPerShard;
                
                // Sidste shard får resten
                if (i == numShards)
                {
                    endDocId = totalDocs;
                }
                
                string shardPath = Path.Combine(dataPath, $"searchDB_shard{i}.db");
                
                Console.WriteLine($"Opretter shard {i}: {shardPath}");
                Console.WriteLine($"  Dokumenter: {startDocId} til {endDocId} ({endDocId - startDocId + 1} docs)");
                
                CreateShard(sourceDb, shardPath, startDocId, endDocId);
                
                Console.WriteLine($"  ✓ Shard {i} oprettet!");
            }
            
            Console.WriteLine($"\n✓ FÆRDIG! {numShards} shards oprettet i {dataPath}");
            Console.WriteLine("\nNæste skridt:");
            Console.WriteLine("1. Start SearchAPI: cd SearchEngine/SearchAPI && dotnet run");
            Console.WriteLine("2. Test query: curl \"http://localhost:5147/api/search?query=meeting&maxResults=10\"");
        }
        
        static int GetDocumentCount(string dbPath)
        {
            using var connection = new SqliteConnection($"Data Source={dbPath}");
            connection.Open();
            
            using var cmd = connection.CreateCommand();
            cmd.CommandText = "SELECT COUNT(*) FROM document";
            
            var result = cmd.ExecuteScalar();
            return Convert.ToInt32(result);
        }
        
        static void CreateShard(string sourceDb, string shardPath, int startDocId, int endDocId)
        {
            // Slet eksisterende shard hvis den findes
            if (File.Exists(shardPath))
            {
                File.Delete(shardPath);
            }
            
            // Opret ny database med samme schema
            using (var shardConn = new SqliteConnection($"Data Source={shardPath}"))
            {
                shardConn.Open();
                
                // Opret tabeller
                using (var cmd = shardConn.CreateCommand())
                {
                    cmd.CommandText = @"
                        CREATE TABLE document(id INTEGER PRIMARY KEY, url TEXT, idxTime TEXT, creationTime TEXT);
                        CREATE TABLE word(id INTEGER PRIMARY KEY, name VARCHAR(50));
                        CREATE TABLE Occ(wordId INTEGER, docId INTEGER, 
                            FOREIGN KEY (wordId) REFERENCES word(id), 
                            FOREIGN KEY (docId) REFERENCES document(id));
                        CREATE INDEX word_index ON Occ (wordId);
                    ";
                    cmd.ExecuteNonQuery();
                }
            }
            
            // Åbn både source og shard
            using var sourceConn = new SqliteConnection($"Data Source={sourceDb}");
            sourceConn.Open();
            
            using var shardConn2 = new SqliteConnection($"Data Source={shardPath}");
            shardConn2.Open();
            
            // Kopier dokumenter
            using (var transaction = shardConn2.BeginTransaction())
            {
                // 1. Kopier dokumenter for dette ID-range
                using (var selectCmd = sourceConn.CreateCommand())
                {
                    selectCmd.CommandText = "SELECT id, url, idxTime, creationTime FROM document WHERE id >= @start AND id <= @end";
                    selectCmd.Parameters.AddWithValue("@start", startDocId);
                    selectCmd.Parameters.AddWithValue("@end", endDocId);
                    
                    using var reader = selectCmd.ExecuteReader();
                    
                    while (reader.Read())
                    {
                        using var insertCmd = shardConn2.CreateCommand();
                        insertCmd.CommandText = "INSERT INTO document (id, url, idxTime, creationTime) VALUES (@id, @url, @idxTime, @creationTime)";
                        insertCmd.Parameters.AddWithValue("@id", reader.GetInt32(0));
                        insertCmd.Parameters.AddWithValue("@url", reader.GetString(1));
                        insertCmd.Parameters.AddWithValue("@idxTime", reader.GetString(2));
                        insertCmd.Parameters.AddWithValue("@creationTime", reader.GetString(3));
                        insertCmd.ExecuteNonQuery();
                    }
                }
                
                // 2. Find alle unikke word IDs brugt i disse dokumenter
                using (var selectCmd = sourceConn.CreateCommand())
                {
                    selectCmd.CommandText = "SELECT DISTINCT wordId FROM Occ WHERE docId >= @start AND docId <= @end";
                    selectCmd.Parameters.AddWithValue("@start", startDocId);
                    selectCmd.Parameters.AddWithValue("@end", endDocId);
                    
                    using var reader = selectCmd.ExecuteReader();
                    var wordIds = new System.Collections.Generic.List<int>();
                    
                    while (reader.Read())
                    {
                        wordIds.Add(reader.GetInt32(0));
                    }
                    
                    // 3. Kopier disse words
                    foreach (var wordId in wordIds)
                    {
                        using var selectWordCmd = sourceConn.CreateCommand();
                        selectWordCmd.CommandText = "SELECT id, name FROM word WHERE id = @id";
                        selectWordCmd.Parameters.AddWithValue("@id", wordId);
                        
                        using var wordReader = selectWordCmd.ExecuteReader();
                        if (wordReader.Read())
                        {
                            using var insertCmd = shardConn2.CreateCommand();
                            insertCmd.CommandText = "INSERT INTO word (id, name) VALUES (@id, @name)";
                            insertCmd.Parameters.AddWithValue("@id", wordReader.GetInt32(0));
                            insertCmd.Parameters.AddWithValue("@name", wordReader.GetString(1));
                            insertCmd.ExecuteNonQuery();
                        }
                    }
                }
                
                // 4. Kopier occurrences for disse dokumenter
                using (var selectCmd = sourceConn.CreateCommand())
                {
                    selectCmd.CommandText = "SELECT wordId, docId FROM Occ WHERE docId >= @start AND docId <= @end";
                    selectCmd.Parameters.AddWithValue("@start", startDocId);
                    selectCmd.Parameters.AddWithValue("@end", endDocId);
                    
                    using var reader = selectCmd.ExecuteReader();
                    
                    while (reader.Read())
                    {
                        using var insertCmd = shardConn2.CreateCommand();
                        insertCmd.CommandText = "INSERT INTO Occ (wordId, docId) VALUES (@wordId, @docId)";
                        insertCmd.Parameters.AddWithValue("@wordId", reader.GetInt32(0));
                        insertCmd.Parameters.AddWithValue("@docId", reader.GetInt32(1));
                        insertCmd.ExecuteNonQuery();
                    }
                }
                
                transaction.Commit();
            }
        }
    }
}
