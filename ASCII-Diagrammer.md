# SearchEngine Arkitektur - Komplet Y-Skalering

## 1. Komplet System Oversigt
### FØR: Monolitisk ConsoleSearch
```
┌─────────────────────────────────┐
│        ConsoleSearch            │
│  ┌─────────────────────────┐    │
│  │         App.cs          │    │
│  │   (User Interface)      │    │
│  └─────────────────────────┘    │
│              │                  │
│              ▼                  │
│  ┌─────────────────────────┐    │
│  │      SearchLogic        │    │
│  │   (Business Logic)      │    │
│  └─────────────────────────┘    │
│              │                  │
│              ▼                  │
│  ┌─────────────────────────┐    │
│  │    DatabaseSqlite       │    │
│  │    (Data Access)        │    │
│  └─────────────────────────┘    │
│              │                  │
│              ▼                  │
│         [SQLite DB]             │
└─────────────────────────────────┘
```
### EFTER: Y-Skaleret Microservices Arkitektur
```
┌─────────────────────────────────────────────────────────────────────────────────┐
│                           FULDT Y-SKALERET SYSTEM                              │
└─────────────────────────────────────────────────────────────────────────────────┘

WEB CLIENT                                    SERVER SIDE
┌─────────────────────────────────┐         ┌─────────────────────────────┐
│         SearchWebApp            │         │        SearchAPI            │
│      (Blazor Server)            │         │    (All Business Logic)     │
│                                 │         │                             │
│ ┌─────────────────────────────┐ │         │ ┌─────────────────────────┐ │
│ │       Search.razor          │ │         │ │   SearchController      │ │
│ │     (Modern Web UI)         │ │         │ │   (REST Endpoints)      │ │
│ └─────────────────────────────┘ │         │ └─────────────────────────┘ │
│              │                  │         │            │                │
│              ▼                  │  HTTP   │            ▼                │
│ ┌─────────────────────────────┐ │  REST   │ ┌─────────────────────────┐ │
│ │    SearchApiService         │◄├─────────┤►│     SearchLogic         │ │
│ │    (HTTP Client)            │ │  API    │ │   (Business Logic)      │ │
│ │    - HttpClient             │ │         │ │   ALL SEARCH LOGIC!     │ │
│ │    - JSON Serialization     │ │         │ │   - Query Processing    │ │
│ │    - POST /api/Search       │ │         │ │   - Database Queries    │ │
│ └─────────────────────────────┘ │         │ │   - Result Filtering    │ │
└─────────────────────────────────┘         │ └─────────────────────────┘ │
                                            │            │                │
DATA CREATION SIDE                          │            ▼                │
┌─────────────────────┐                     │ ┌─────────────────────────┐ │
│      Indexer        │                     │ │    DatabaseSqlite       │ │
│   (Data Crawler)    │                     │ │    (Data Access)        │ │
│                     │                     │ └─────────────────────────┘ │
│ ┌─────────────────┐ │                     │            │                │
│ │      App.cs     │ │────┐                │            ▼                │
│ │   (Indexing)    │ │    │                │       [SQLite DB]           │
│ └─────────────────┘ │    │                └─────────────────────────────┘
│          │          │    │                              ▲
│          ▼          │    │                              │
│ ┌─────────────────┐ │    │ WRITES                       │ READS
│ │    Crawler      │ │    │ DATABASE                     │
│ │  (File Parser)  │ │    │                              │
│ └─────────────────┘ │    │                              │
│          │          │    │                              │
│          ▼          │    │                              │
│ ┌─────────────────┐ │    │                              │
│ │ DatabaseSqlite  │◄├────┘                              │
│ │ (Write to DB)   │ │                                   │
│ └─────────────────┘ │                                   │
└─────────────────────┘                                   │
          │                                               │
          ▼                                               │
    [Data Files]                                          │
    ┌─────────────┐                                       │
    │small/       │───────────────────────────────────────┘
    │medium/      │
    │large/       │
    └─────────────┘
```
## 2. Data Flow - Simpel 3-Tier Arkitektur
```
┌─────────────┐                                      ┌─────────────┐
│   INDEXER   │    ┌─────────────┐   1. User Search  │   BLAZOR    │
│             │    │   Browser   │   Query Input     │   WebApp    │
│             │    │             │ ─────────────────►│             │
│             │    │             │ ◄─────────────────│             │
└─────────────┘    └─────────────┘   6. HTML Result  └─────────────┘
       │                   │                                │
       │                   │ 2. HTTP Request                │ 2. HTTP  
       │ 0. CREATE         │    POST /api/Search            │    Request
       │    INDEX          │    JSON: {query, maxResults}   │    
       │                   │                                │
       ▼                   ▼                                ▼
┌─────────────┐    ┌─────────────────────────────────────────────────┐
│ SQLite DB   │◄───│               SearchAPI                         │
│             │    │         (Central Business Logic)                │
│             │    │                                                 │
│[Documents]  │    │  ┌─────────────┐  ┌─────────────────────────┐   │
│[Words]      │    │  │SearchContr. │  │     SearchLogic         │   │
│[Occurrences]│    │  │(REST API)   │  │  - Query Processing     │   │
└─────────────┘    │  │ + POST      │  │  - Database Queries     │   │
       ▲           │  │ + GET       │  │  - Result Filtering     │   │
       │           │  └─────────────┘  │  - Business Rules       │   │
       │           │          │        └─────────────────────────┘   │
       │           │          ▼ 3. Call Business Logic               │
       │           │  ┌─────────────┐              │                  │
       │           │  │DatabaseSqlite│             │                 │
       │ 4. Query  │  │(Data Access) │◄────────────┘                 │ 
       │    DB     │  └─────────────┘                                │
       └───────────┼─────────────────────────────────────────────────┘
                   │          │
                   │          ▼ 5. JSON Response
                   │          │    SearchResult object
                   │          │
                   │  ┌─────────────┐
                   └─►│SearchApiSrv │
                      │(HTTP Client)│
                      └─────────────┘

CLEAN 3-TIER ARKITEKTUR:
✅ Presentation Tier = Blazor UI (SearchWebApp)
✅ Business Tier     = SearchAPI (All Logic)  
✅ Data Tier         = SQLite Database + Files
✅ Indexer           = Separate Data Pipeline
```

## 3. Komplet Separation - Hvad Ligger Hvor?

```
┌────────────────────────────────────────────────────────────────┐
│                    EFTER Y-SKALERING                           │
├────────────────────────────────────────────────────────────────┤
│                                                                │
│  BLAZOR CLIENT (Pure UI)            SERVER API (All Logic)     │
│  ┌─────────────────────────┐       ┌─────────────────────────┐ │
│  │ SearchWebApp Project    │       │ SearchAPI Project       │ │
│  │                         │       │                         │ │
│  │ ✅ Search.razor         │       │ ✅ SearchController    │ │
│  │   - Modern Blazor UI    │       │   - REST endpoints      │ │
│  │   - Interactive forms   │       │   - HTTP handling       │ │
│  │                         │       │                         │ │
│  │ ✅ SearchApiService    │   ────┤ ✅ SearchLogic          │ │
│  │   - HTTP client ONLY    │  HTTP │   - ALL search logic    │ │
│  │   - JSON serialization  │  REST │   - Query processing    │ │
│  │   - No business logic   │   API │   - Result filtering    │ │
│  │                         │       │                         │ │
│  │ ❌ NO SearchLogic      │        │ ✅ DatabaseSqlite       │ │
│  │ ❌ NO Database         │        │   - Data access layer    │ │
│  │ ❌ NO File access       │       │   - SQLite operations   │ │
│  │                         │        │                         │ │
│  │                         │        │ ✅ IDatabase interface  │ │
│  └─────────────────────────┘        │                         │ │
│                                     │ ✅ File System Access  │ │
│                                     │   - Document reading    │ │
│                                     │   - Path handling       │ │
│                                     └─────────────────────────┘ │
└─────────────────────────────────────────────────────────────────┘

## 4. Klasse Diagram - 3-Tier Arkitektur

```
PRESENTATION TIER                    BUSINESS TIER                      DATA TIER & INDEXING
┌─────────────────────────────┐     ┌─────────────────────────────┐    ┌─────────────────────────────┐
│       SearchWebApp          │     │        SearchAPI            │    │         Indexer             │
│     (Blazor Client)         │     │    (Business Logic)         │    │     (Data Pipeline)         │
│                             │     │                             │    │                             │
│ ┌─────────────────────────┐ │     │ ┌─────────────────────────┐ │    │ ┌─────────────────────────┐ │
│ │     Search.razor        │ │     │ │   SearchController      │ │    │ │        App.cs           │ │
│ │   (Blazor Component)    │ │     │ │   (REST Endpoints)      │ │    │ │     (Indexing)          │ │
│ │  - SearchModel          │ │     │ │  + GET Search()         │ │    │ └─────────────────────────┘ │
│ │  - isSearching          │ │     │ │  + POST Search()        │ │    │             │               │
│ │  + PerformSearch()      │ │     │ │  - SearchLogic logic    │ │    │             ▼               │
│ │  + HandleKeyPress()     │ │     │ └─────────────────────────┘ │    │ ┌─────────────────────────┐ │
│ └─────────────────────────┘ │     │             │               │    │ │       Crawler           │ │
│             │               │HTTP │             ▼               │    │ │    (File Parser)        │ │
│             ▼               │REST │ ┌─────────────────────────┐ │    │ │  - separators[]         │ │
│ ┌─────────────────────────┐ │API  │ │     SearchLogic         │ │    │ │  - words Dictionary     │ │
│ │   SearchApiService      │◄├─────┤►│  (Business Logic)       │ │    │ │  + ExtractWordsInFile() │ │
│ │    (HTTP Client)        │ │     │ │  - IDatabase database   │ │    │ │  + IndexFilesIn()       │ │
│ │  - HttpClient           │ │     │ │  + Search()             │ │    │ └─────────────────────────┘ │
│ │  - apiBaseUrl           │ │     │ │  + FilterResults()      │ │    │             │               │
│ │  + SearchAsync()        │ │     │ │  + ApplySettings()      │ │    │             ▼               │
│ │  + IsApiAvailable()     │ │     │ └─────────────────────────┘ │    │ ┌─────────────────────────┐ │
│ └─────────────────────────┘ │     │             │               │    │ │    DatabaseSqlite       │ │
│                             │     │             ▼               │    │ │    (Write Access)       │ │
│ ❌ NO Business Logic        │     │ ┌─────────────────────────┐ │    │ │  + InsertDocument()     │ │
│ ❌ NO Database Access       │     │ │     IDatabase           │ │    │ │  + InsertWord()         │ │
│ ❌ NO File System Access    │     │ │    <<interface>>        │ │    │ │  + InsertAllWords()     │ │
└─────────────────────────────┘     │ └─────────────────────────┘ │    │ │  + InsertAllOcc()       │ │
                                    │             │               │    │ └─────────────────────────┘ │
SHARED DATA MODELS                  │             ▼               │    └─────────────────────────────┘
┌───────────────────────────────┐   │ ┌─────────────────────────┐ │                 │
│         Shared.Model          │   │ │   DatabaseSqlite        │ │                 │
│                               │◄──┼─┤   (Read Access)         │ │                 │
│ ┌─────────────────────────┐   │   │ │  + GetDocuments()       │ │                 ▼
│ │    SearchResult         │   │   │ │  + ExecuteQuery()       │ │         [Data Files]
│ │  + Query: string[]      │   │   │ │  + WordsFromIds()       │ │       ┌───────────────┐
│ │  + DocumentHits         │   │   │ │  + getMissing()         │ │       │    small/     │
│ │  + Hits: int            │   │   │ └─────────────────────────┘ │       │    medium/    │
│ │  + TimeUsed             │   │   │             │               │       │    large/     │
│ │  + Ignored              │   │   │             ▼               │       └───────────────┘
│ └─────────────────────────┘   │   │      [SQLite DB]            │                 │
│                               │   │   ┌───────────────────┐     │                 │
│ ┌─────────────────────────┐   │   │   │    Tables:        │     │                 │
│ │    DocumentHit          │   │   │   │  - document       │◄────┼─────────────────┘
│ │  + Document             │   │   │   │  - word           │     │        READS/WRITES
│ │  + NoOfHits             │   │   │   │  - Occ            │     │        SAME DATABASE
│ │  + Missing              │   │   │   └───────────────────┘     │
│ └─────────────────────────┘   │   └─────────────────────────────┘
│                               │
│ ┌─────────────────────────┐   │
│ │    BEDocument           │   │   JSON SERIALIZATION
│ │  + mId: int             │   │   (HTTP Communication)
│ │  + mUrl: string         │   │
│ │  + mIdxTime: string     │   │
│ │  + mCreationTime        │   │
│ └─────────────────────────┘   │
└───────────────────────────────┘

PERFEKT SEPARATION OF CONCERNS:
✅ Presentation = Blazor UI (Kun HTTP kald og visning)
✅ Business     = SearchAPI (Al søgelogik og forretningsregler)
✅ Data         = SQLite Database (Persistering)
✅ Indexing     = Separat pipeline (Data preparation)
```

## 5. Y-Skalering Migration - Hvad Blev Flyttet Hvor?

```
┌─────────────────────────────────────────────────────────────────────────────────┐
│                          MIGRATION OVERSIGT                                     │
└─────────────────────────────────────────────────────────────────────────────────┘

FØR: Monolitisk ConsoleSearch        EFTER: 4 Separate Projekter
┌─────────────────────────────┐      
│      ConsoleSearch          │      ┌─────────────────────┐
│      (Monolith)             │      │  ConsoleSearch      │
│                             │      │  (Pure HTTP Client) │
│ ├── App.cs                  │ ────►│  ├── App.cs         │ ✅ UI ONLY
│ ├── SearchLogic.cs          │ ──┐  │  ├── ApiSearchLogic │ ✅ HTTP ONLY  
│ ├── DatabaseSqlite.cs       │   │  │  └── Config.cs      │
│ ├── IDatabase.cs            │   │  └─────────────────────┘
│ ├── DocumentHit.cs          │   │  
│ ├── SearchResult.cs         │   │  ┌─────────────────────┐
│ ├── BEDocument.cs           │   │  │   SearchWebApp      │
│ └── Config.cs               │   │  │   (Blazor Client)   │
└─────────────────────────────┘   │  │  ├── Search.razor   │ ✅ BLAZOR UI
                                  │  │  ├── SearchApiSrv   │ ✅ HTTP SERVICE
                  ┌───────────────┘  │  └── Program.cs     │
                  │                  └─────────────────────┘
                  ▼ 
         ┌─────────────────────┐
         │    SearchAPI        │
         │   (All Logic)       │✅ REST API
         │                     │✅ BUSINESS LOGIC
         │  SearchLogic.cs     │✅ DATA ACCESS
         │  DatabaseSqlite     │✅ INTERFACE
         │  IDatabase.cs       │
         │  +SearchController  │
         └─────────────────────┘     
                                     ┌─────────────────────┐
                                     │      Shared         │
          DocumentHit.cs      ────►  │  ├── SearchResult   │ ✅ DATA MODELS
          SearchResult.cs     ────►  │  ├── DocumentHit    │ ✅ JSON DTOs
          BEDocument.cs       ────►  │  ├── BEDocument     │ ✅ ENTITIES
                                     │  └── Config/Paths   │ ✅ UTILITIES
                                     └─────────────────────┘

┌─────────────────────────────────────────────────────────────────────────────────┐
│                           Y-SKALERING RESULTATER                               │
├─────────────────────────────────────────────────────────────────────────────────┤
│                                                                                 │
│ ✅ ConsoleSearch  → Pure HTTP client (INGEN business logic)                    │
│ ✅ SearchWebApp   → Blazor UI (samme HTTP client pattern)                      │
│ ✅ SearchAPI      → Central server (ALL business logic)                        │
│ ✅ Indexer        → Separate data creation service                             │
│ ✅ Shared         → Common data models (JSON communication)                    │
│                                                                                 │
│ 🎯 PERFECT SEPARATION OF CONCERNS OPNÅET!                                      │
└─────────────────────────────────────────────────────────────────────────────────┘
```

## 6. Deployment Diagrammer

### DEVELOPMENT Environment
```
┌──────────────────────────────────────────────────────────────────────────┐
│                           LOCALHOST DEVELOPMENT                          │
├──────────────────────────────────────────────────────────────────────────┤
│                                                                          │
│  ┌─────────────────┐                                                     │
│  │ SearchWebApp    │                                                     │
│  │ :5000 (Blazor)  │                                                     │
│  └─────────────────┘                                                     │
│           │                                                              │
│           │ HTTP POST                                                    │
│           │ /api/Search                                                  │
│           ▼                                                              │
│  ┌──────────────────────────────────────────────────────────────────┐    │
│  │                    SearchAPI :5281                               │    │
│  │               (ASP.NET Core Web API)                             │    │
│  │                                                                  │    │
│  │  ┌─────────────────┐    ┌─────────────────────────────────┐      │    │
│  │  │SearchController │    │        SearchLogic              │      │    │
│  │  │(REST Endpoints) │───►│    (Business Logic)             │      │    │
│  │  └─────────────────┘    └─────────────────────────────────┘      │    │
│  └──────────────────────────────────────────────────────────────────┘    │
│                                    │                                     │
│                                    │ SQLite Connection                   │
│                                    ▼                                     │
│  ┌─────────────────┐    ┌─────────────────────────────────────────┐      │
│  │   Data Files    │    │            Database                     │      │
│  │   ├─ small/     │    │      searchDBmedium.db                  │      │
│  │   ├─ medium/    │◄──►│     ┌─────────────────────────────┐     │      │
│  │   └─ large/     │    │     │ Tables:                    │      │      │
│  └─────────────────┘    │     │ - document (id,url,time)   │      │      │
│           ▲             │     │ - word (id,name)           │      │      │
│           │             │     │ - Occ (wordId,docId)       │      │      │
│           │             │     └─────────────────────────────┘     │      │
│           └─────────────┤                                         │      │
│        Indexer Writes   └─────────────────────────────────────────┘      │
│        SearchAPI Reads                                                   │
└──────────────────────────────────────────────────────────────────────────┘

SIMPEL 2-KOMPONENT SYSTEM:
✅ SearchWebApp → Blazor frontend (port 5000)
✅ SearchAPI    → Business logic server (port 5281) 
✅ Indexer      → Data preparation (køres separat)
✅ Database     → Shared SQLite storage
```

### PRODUCTION Deployment (Skalerbar)
```
┌──────────────────────────────────────────────────────────────────────────┐
│                            PRODUCTION CLOUD                              │
├──────────────────────────────────────────────────────────────────────────┤
│                                                                          │
│  ┌─────────────────┐    ┌─────────────────┐    ┌─────────────────┐      │
│  │ Blazor WebApp   │    │  Mobile App     │    │ Desktop Client  │      │
│  │ (Azure Static)  │    │  (React/Vue)    │    │ (WPF/Console)   │      │
│  └─────────────────┘    └─────────────────┘    └─────────────────┘      │
│           │                       │                       │              │
│           └───────────────────────┼───────────────────────┘              │
│                                   │ HTTPS                                 │
│                                   ▼                                      │
│                      ┌─────────────────────┐                            │
│                      │   Load Balancer     │                            │
│                      │  (Azure Gateway)    │                            │
│                      └─────────────────────┘                            │
│                                   │                                      │
│              ┌────────────────────┼────────────────────┐                │
│              │                    │                    │                │
│              ▼                    ▼                    ▼                │
│     ┌─────────────────┐ ┌─────────────────┐ ┌─────────────────┐        │
│     │ SearchAPI Pod1  │ │ SearchAPI Pod2  │ │ SearchAPI Pod3  │        │
│     │ (Kubernetes)    │ │ (Kubernetes)    │ │ (Kubernetes)    │        │
│     └─────────────────┘ └─────────────────┘ └─────────────────┘        │
│              │                    │                    │                │
│              └────────────────────┼────────────────────┘                │
│                                   │                                      │
│                                   ▼                                      │
│              ┌─────────────────────────────────────────┐                │
│              │         Shared Database Cluster         │                │
│              │        (Azure SQL / PostgreSQL)        │                │
│              │                                         │                │
│              │  ┌─────────────┐ ┌─────────────┐       │                │
│              │  │ Primary DB  │ │ Replica DB  │       │                │
│              │  │ (Write)     │ │ (Read)      │       │                │
│              │  └─────────────┘ └─────────────┘       │                │
│              └─────────────────────────────────────────┘                │
│                                   │                                      │
│                                   ▼                                      │
│              ┌─────────────────────────────────────────┐                │
│              │          File Storage                   │                │
│              │       (Azure Blob / S3)                │                │
│              │    ┌─────────┐ ┌─────────┐             │                │
│              │    │Documents│ │ Indexes │             │                │
│              │    │         │ │         │             │                │
│              │    └─────────┘ └─────────┘             │                │
│              └─────────────────────────────────────────┘                │
└──────────────────────────────────────────────────────────────────────────┘

SKALERINGS FORDELE:
✅ Horisontale skalering - Flere API instanser
✅ Load balancing - Fordel trafik
✅ Database replicas - Læse-performance 
✅ Microservices - Uafhængig deployment
✅ Cloud native - Auto-scaling
```