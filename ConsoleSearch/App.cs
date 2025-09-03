using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ConsoleSearch
{
    public class App
    {
        private ApiSearchLogic _apiSearchLogic;

        public App()
        {
            _apiSearchLogic = new ApiSearchLogic();
        }

        public async Task RunAsync()
        {
            Console.WriteLine("Console Search (Pure API Client)");
            Console.WriteLine($"Case sensitive search: {(Config.CaseSensitive ? "ON" : "OFF")}");
            Console.WriteLine($"View timestamps: {(Config.ViewTimeStamps ? "ON" : "OFF")}");
            Console.WriteLine($"Max results: {(Config.MaxResults?.ToString() ?? "ALL")}");
            Console.WriteLine("Commands: /casesensitive=on or /casesensitive=off, /timestamp=on or /timestamp=off, /results=X or /results=all");
            Console.WriteLine("Mode: API Only - All search logic is on the server");
            
            while (true)
            {
                Console.WriteLine("enter search terms - q for quit");
                string input = Console.ReadLine();
                if (input.Equals("q")) break;

                // Check for commands
                if (input.StartsWith("/casesensitive="))
                {
                    string value = input.Substring(15).ToLower();
                    if (value == "on")
                    {
                        Config.CaseSensitive = true;
                        Console.WriteLine("Case sensitive search: ON");
                    }
                    else if (value == "off")
                    {
                        Config.CaseSensitive = false;
                        Console.WriteLine("Case sensitive search: OFF");
                    }
                    else
                    {
                        Console.WriteLine("Invalid command. Use /casesensitive=on or /casesensitive=off");
                    }
                    continue;
                }

                if (input.StartsWith("/timestamp="))
                {
                    string value = input.Substring(11).ToLower();
                    if (value == "on")
                    {
                        Config.ViewTimeStamps = true;
                        Console.WriteLine("View timestamps: ON");
                    }
                    else if (value == "off")
                    {
                        Config.ViewTimeStamps = false;
                        Console.WriteLine("View timestamps: OFF");
                    }
                    else
                    {
                        Console.WriteLine("Invalid command. Use /timestamp=on or /timestamp=off");
                    }
                    continue;
                }

                if (input.StartsWith("/results="))
                {
                    string value = input.Substring(9).ToLower();
                    if (value == "all")
                    {
                        Config.MaxResults = null;
                        Console.WriteLine("Max results: ALL");
                    }
                    else if (int.TryParse(value, out int resultCount) && resultCount > 0)
                    {
                        Config.MaxResults = resultCount;
                        Console.WriteLine($"Max results: {resultCount}");
                    }
                    else
                    {
                        Console.WriteLine("Invalid command. Use /results=X (where X is a positive number) or /results=all");
                    }
                    continue;
                }

                // Perform search via API only
                var query = input.Split(" ", StringSplitOptions.RemoveEmptyEntries);
                
                int maxResults = Config.MaxResults ?? int.MaxValue;
                
                var result = await _apiSearchLogic.SearchAsync(query, maxResults);
                
                if (result == null)
                {
                    Console.WriteLine("Failed to get results from API. Make sure SearchAPI is running on http://localhost:5281");
                    continue;
                }

                // Display results - same format as before
                if (result.Ignored.Count > 0) {
                    Console.WriteLine($"Ignored: {string.Join(',', result.Ignored)}");
                }

                int idx = 1;
                foreach (var doc in result.DocumentHits) {
                    Console.WriteLine($"{idx} : {doc.Document.mUrl} -- contains {doc.NoOfHits} search terms");
                    
                    if (Config.ViewTimeStamps)
                    {
                        Console.WriteLine("Index time: " + doc.Document.mIdxTime);
                    }
                    Console.WriteLine($"Missing: {ArrayAsString(doc.Missing.ToArray())}");
                    idx++;
                }
                Console.WriteLine("Documents: " + result.Hits + ". Time: " + result.TimeUsed.TotalMilliseconds);
            }
        }

        string ArrayAsString(string[] s) {
            return s.Length == 0?"[]":$"[{String.Join(',', s)}]";
        }
    }
}
