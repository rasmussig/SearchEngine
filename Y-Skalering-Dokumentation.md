# Y-skalering af ConsoleSearch - Dokumentation

### Hvad vi har bygget:

#### 1. **Web API (SearchAPI)** - Server
- **SearchController** - REST API endpoints for søgning
- **SearchLogic** - Al søgelogik flyttet fra ConsoleSearch til serveren
- **DatabaseSqlite** - Database adgang kun på serveren
- **IDatabase interface** - Database abstraktion

#### 2. **Konsol Applikation (ConsoleSearch)** - Ren Klient
- **ApiSearchLogic** - HTTP client til at kommunikere med API'et
- **App.cs** - Kun brugerinteraktion og API kald
- **Ingen lokal søgelogik** - Alt søgning sker via API

#### 3. **Shared Models**
- **BEDocument** - Dokument model med JSON serialization
- **DocumentHit** - Søgeresultat per dokument
- **SearchResult** - Komplet søgeresultat
- **Config & Paths** - Delt konfiguration

### Arkitektur efter y-skalering:

```
ConsoleSearch (Pure Client)          SearchAPI (Server)
├── App.cs (UI only)                ├── SearchController (REST API)
├── ApiSearchLogic (HTTP client) ───┤ ├── SearchLogic (Business Logic)
└── Config.cs                       ├── DatabaseSqlite (Data Access)
                                     └── IDatabase (Interface)
        │
        └── Shared Models ←──────────┘
            ├── BEDocument
            ├── DocumentHit  
            ├── SearchResult
            └── Config/Paths
```

### ✅ Komplet Separation Opnået:

1. **ConsoleSearch** - Indeholder KUN:
   - Brugerinteraktion (input/output)
   - HTTP kommunikation til API
   - Ingen database logik
   - Ingen søgealgoritmer

2. **SearchAPI** - Indeholder AL søgelogik:
   - Database operationer
   - Søgealgoritmer
   - Forretningslogik
   - JSON serialization

### Fordele ved denne rene løsning:

1. **Perfekt Separation of Concerns** - UI og logik er 100% adskilt
2. **Skalerbarhed** - API kan håndtere mange klienter samtidigt
3. **Teknologi frihed** - Andre klienter (web, mobile) kan bruge samme API
4. **Centraliseret logik** - Alle ændringer sker kun ét sted (API)
5. **Testbarhed** - API og klient kan testes uafhængigt

### Sådan køres systemet:

1. **Start API serveren:**
   ```bash
   cd SearchAPI
   dotnet run
   ```
   - API kører på http://localhost:5281

2. **Start klient applikationen:**
   ```bash
   cd ConsoleSearch  
   dotnet run
   ```

3. **Brug konsol kommandoer:**
   - `/casesensitive=on|off` - Case sensitivity
   - `/timestamp=on|off` - Vis tidsstempler
   - `/results=X|all` - Maksimale resultater
   - Normale søgetermer - f.eks. "peter"

### Tekniske detaljer:

#### API Endpoints:
- `GET /api/Search?query=searchterm&maxResults=20` - Simple søgning via URL
- `POST /api/Search` - Avanceret søgning via JSON body

#### Projektstruktur efter Y-skalering:
```
SearchEngine/
├── ConsoleSearch/                 # Pure API Client
│   ├── App.cs                    # UI logic only
│   ├── ApiSearchLogic.cs         # HTTP client
│   ├── Config.cs                 # Client configuration
│   └── ConsoleSearch.csproj      # No database dependencies
├── SearchAPI/                     # Server with all logic
│   ├── Controllers/
│   │   └── SearchController.cs   # REST API endpoints
│   ├── SearchLogic.cs            # Business logic
│   ├── DatabaseSqlite.cs         # Data access
│   ├── IDatabase.cs              # Interface
│   └── SearchAPI.csproj          # Web API project
├── Shared/                        # Common models
│   ├── Model/
│   │   ├── BEDocument.cs         # Document model
│   │   ├── DocumentHit.cs        # Search hit model
│   │   └── SearchResult.cs       # Result model
│   ├── Config.cs                 # Shared config
│   └── Paths.cs                  # Path utilities
└── indexer/                       # Document indexer
    └── (unchanged)
```

#### Fjernede filer fra ConsoleSearch:
- ❌ `SearchLogic.cs` - Flyttet til SearchAPI
- ❌ `DatabaseSqlite.cs` - Flyttet til SearchAPI  
- ❌ `IDatabase.cs` - Flyttet til SearchAPI
- ❌ `DocumentHit.cs` - Flyttet til Shared
- ❌ `SearchResult.cs` - Flyttet til Shared

#### Vigtige rettelser:
1. **JSON Serialization Fix:**
   - Tilføjet `IncludeFields = true` i `JsonSerializerOptions`
   - Dette var nøglen til at få filstier til at vises korrekt

2. **Model Migration:**
   - Flyttet `SearchResult` og `DocumentHit` til `Shared.Model`
   - Tilføjet JSON attributter for korrekt serialization

3. **Dependency Cleanup:**
   - Fjernet SQLite dependencies fra ConsoleSearch.csproj
   - ConsoleSearch afhænger nu kun af HTTP client og JSON

### Konfiguration:

#### API URL:
Standard API URL er `http://localhost:5281`. Dette kan ændres i `ApiSearchLogic` konstruktøren.

#### JSON Serialization:
Alle model klasser har JSON attributter:
```csharp
[JsonConstructor]
[JsonPropertyName("propertyName")]
```

#### Cross-machine Compatibility:
- Alle stier er nu relative til project root
- `Paths.ProjectRoot` finder automatisk project roden
- Virker på tværs af maskiner og brugerkonti

Dette sikrer korrekt serialization/deserialization mellem API og client.

#### Database sti:
Bruger nu relative stier via `Shared/Paths.cs` som automatisk finder Data mappen relative til projektets placering.

### Kommandoer i ConsoleSearch:

#### Eksisterende kommandoer:
- `/casesensitive=on|off` - Skifter case sensitivity
- `/timestamp=on|off` - Viser/skjuler timestamps
- `/results=X|all` - Sætter max antal resultater

#### Nye kommandoer:
- `/mode=api` - Skifter til API mode (standard)
- `/mode=local` - Skifter til lokal søgelogik mode

### 🎯 Resultat:

**Y-skaleringen er nu komplet implementeret!**

✅ **Perfekt separation** - ConsoleSearch indeholder 0% søgelogik  
✅ **Ren arkitektur** - Client-server med REST API  
✅ **Filstier virker** - API viser fulde stier korrekt  
✅ **JSON serialization** - Korrekt data transmission  
✅ **Cross-platform** - Relative stier på tværs af maskiner  

**Før Y-skalering:** Monolitisk konsol applikation  
**Efter Y-skalering:** Distribueret client-server arkitektur

Dette er et perfekt eksempel på Y-skalering hvor:
- **Presentation Layer** (ConsoleSearch) er adskilt fra
- **Business Logic Layer** (SearchAPI)
- Via en **Service Layer** (REST API)

### Fejlløsning udført:

#### Problem med JSON Deserialization:
**Fejl:** "Each parameter in the deserialization constructor must bind to an object property"

**Løsning:** 
- Tilføjet `IncludeFields = true` i JsonSerializerOptions
- Flyttet model klasser til Shared projekt
- Tilføjet JSON attributter til alle model klasser

#### Problem med tomme filstier:
**Fejl:** Filstier blev ikke vist i ConsoleSearch output

**Løsning:**
- `IncludeFields = true` var nøglen - JSON kunne nu læse fields
- BEDocument model blev korrekt deserialiseret
- Filstier vises nu perfekt: `C:\Users\...\file.txt`

### 📝 Næste skridt:
- Overvej at tilføje flere API endpoints
- Implementer eventuelt web-baseret klient  
- Tilføj logging og monitoring til API'et
- Overvej at implementere caching i API'et
- Sikret at parameter navne matcher JSON property navne

Nu kan din searchengine skalere horisontalt ved at køre flere instanser af API'et! 🚀

### Næste skridt for yderligere skalering:

1. **Load balancer** - For at distribuere requests mellem flere API instanser
2. **Caching** - Redis eller lignende for at cache søgeresultater
3. **Database connection pooling** - For bedre database performance
4. **Logging og monitoring** - For at overvåge API performance
5. **Configuration management** - Eksternalisering af konfiguration
