This codebase is a PoC searchengine that consist of multiple programs and a class library.

## 📚 FULL DOCUMENTATION

**For complete usage instructions, see:** `GUIDE.md`

The guide covers:
- Indexing data
- Database sharding (Y-scaling)
- Running SearchAPI
- Load balancing with multiple instances
- Full demo setups

---

## QUICK START

```bash
# 1. Index data
cd indexer && dotnet run

# 2. Start SearchAPI
cd ../SearchAPI && dotnet run

# 3. Test search
curl "http://localhost:5147/api/search?query=meeting&maxResults=10"
```

---

## COMPONENTS

The two main programs are the indexer (also called a crawler) and SearchAPI (REST API).

**Indexer** - Crawls folders and creates reverse index in SQLite database
**SearchAPI** - REST API for searching (with auto-detection of database shards)
**DatabaseSplitter** - Splits database into shards for Y-scaling
**LoadBalancer** - Distributes requests across multiple SearchAPI instances
**ConsoleSearch** - Legacy console search interface (deprecated, use SearchAPI)

The class library Shared contains classes that are used by the indexer
and SearchAPI. It contains:

- Paths containing static paths to databases (used by both indexer and SearchAPI)
- BEDocument (BE for Business Entity) - a class representing a document
- Config containing global configuration settings

## SETUP INSTRUCTIONS

### Test Data Setup
1. Download test data from releases: https://github.com/rasmussig/SearchEngine/releases/download/v1.0.0/Data.zip
2. Extract the Data.zip file to the same level as the SearchEngine folder:
   ```
   YourFolder/
   ├── SearchEngine/     (this project)
   └── Data/            (extracted test data)
       ├── small/
       ├── medium/
       └── large/
   ```

### Running the Application
1. Update Shared.Paths so the database path points to the correct database location
2. The indexer will automatically find the Data folder using the Config.cs setup
3. Run indexer (dotnet run)
4. Inspect database - check that all documents are indexed and verify word indexing
5. Run searchConsole and test with queries (1 word, 2 words, multiple words)

## NEW FEATURES (Version 2)

### Opgave 2: Enhanced Word Frequency Output
CHANGED FILES:
- indexer/DatabaseSqlite.cs: Added GetWordFrequencies() method
- indexer/App.cs: Modified to show word frequencies sorted by occurrence count

The indexer now shows word frequencies instead of just word IDs, with most frequent words first.
Instead of showing <word, id>, it now shows <word, id> - frequency_count

### Opgave 3: Case Sensitive Search Control
CHANGED FILES:
- Shared/Config.cs: Added CaseSensitive boolean property
- ConsoleSearch/Config.cs: Added wrapper for CaseSensitive property
- ConsoleSearch/App.cs: Added command handling for /casesensitive=on/off

Users can now control case sensitivity with commands:
- /casesensitive=on - enables case sensitive search
- /casesensitive=off - disables case sensitive search

### Opgave 4: Timestamp Display Control
CHANGED FILES:
- Shared/Config.cs: Added ViewTimeStamps boolean property
- ConsoleSearch/Config.cs: Added wrapper for ViewTimeStamps property  
- ConsoleSearch/App.cs: Added command handling for /timestamp=on/off and conditional timestamp display

Users can now control timestamp visibility with commands:
- /timestamp=on - shows indexing timestamps in search results
- /timestamp=off - hides indexing timestamps in search results

### Opgave 5: Configurable Search Results Limit
CHANGED FILES:
- Shared/Config.cs: Added MaxResults int? property (default 20, null = show all)
- ConsoleSearch/Config.cs: Added wrapper for MaxResults property
- ConsoleSearch/App.cs: Added command handling for /results=X or /results=all and dynamic result limiting

Users can now control the number of search results with commands:
- /results=X - shows X number of results (e.g., /results=15)
- /results=all - shows all available results
Default changed from 10 to 20 results per search.

### Opgave 6: Load Balancer for SearchAPI
CHANGED FILES:
- LoadBalancer/Controllers/SearchController.cs: Implements random load balancing with health checks
- LoadBalancer/Program.cs: ASP.NET Core setup with HttpClient support

A load balancer that distributes search requests across multiple SearchAPI instances.

FEATURES:
- Random load balancing strategy across healthy backend instances
- Health check endpoint (/api/Search/ping) to verify backend availability
- Automatic failover - only forwards to healthy backends
- Response headers showing which backend handled each request (X-LoadBalancer-Backend)

HOW TO TEST:
1. Start multiple SearchAPI instances on different ports:
   Terminal 1: id=SearchAPI_1 dotnet run --urls "http://localhost:5281" (in SearchAPI folder)
   Terminal 2: id=SearchAPI_2 dotnet run --urls "http://localhost:5282" (in SearchAPI folder)

2. Start LoadBalancer:
   Terminal 3: dotnet run (in LoadBalancer folder, runs on port 5280)

3. Check backend status:
   curl http://localhost:5280/api/Search/status

4. Test search through LoadBalancer:
   curl "http://localhost:5280/api/Search?query=test&maxResults=5"

5. Verify load distribution (see X-LoadBalancer-Backend header):
   curl -i "http://localhost:5280/api/Search?query=test&maxResults=5"

6. Test failover by stopping one SearchAPI instance and verify LoadBalancer only uses remaining backends.

## COMMANDS AVAILABLE IN CONSOLE SEARCH

- /casesensitive=on/off - Toggle case sensitive search
- /timestamp=on/off - Toggle timestamp display in results
- /results=X or /results=all - Set maximum number of search results to show
- q - Quit the application