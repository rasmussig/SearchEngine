using System;
using System.Collections.Generic;
using System.IO;
using Shared;

namespace Indexer
{
    public class App
    {
        public void Run(){
            DatabaseSqlite db = new DatabaseSqlite(Paths.DATABASE);
            Crawler crawler = new Crawler(db);

            var root = new DirectoryInfo(Config.FOLDER);

            DateTime start = DateTime.Now;

            crawler.IndexFilesIn(root, new List<string> { ".txt"});        

            TimeSpan used = DateTime.Now - start;
            Console.WriteLine("DONE! used " + used.TotalMilliseconds);

            // Hent ord med deres faktiske forekomster
            var wordFrequencies = db.GetWordFrequencies();

            Console.WriteLine($"Indexed {db.DocumentCounts} documents");
            Console.WriteLine($"Number of different words: {wordFrequencies.Count}");

            // Ask user how many most frequent words they want to see
            Console.WriteLine("How many most frequent words would you like to see?");
            string input = Console.ReadLine();
            int wordsToShow = 10; // default
            
            if (!int.TryParse(input, out wordsToShow) || wordsToShow < 1)
            {
                wordsToShow = 10;
                Console.WriteLine("Invalid input. Showing 10 words.");
            }
            
            Console.WriteLine($"The {wordsToShow} most frequent words are:");
            int count = 0;
            foreach (var p in wordFrequencies)
            {
                Console.WriteLine($"<{p.Key}, {p.Value}>");
                count++;
                if (count == wordsToShow) break;
            }
        }
    }
}
