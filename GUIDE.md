# Search Engine - Komplet Brugsguide

Dette dokument beskriver hvordan du bruger hele search engine systemet fra start til slut.

**📋 Indholdsfortegnelse:**
- [Quick Start](#-quick-start-basis-setup) - Kom hurtigt i gang med basis søgning
- [Y-Skalering](#-avanceret-y-skalering-database-sharding) - Database sharding setup
- [Load Balancing](#️-load-balancing-multiple-api-instances) - Kør multiple API instances
- [Fuld Demo](#-fuld-demo-setup-y-skalering--load-balancing) - Alt på én gang
- [API Reference](#-api-endpoints) - Endpoint dokumentation
- [Troubleshooting](#️-troubleshooting) - Løsning af problemer
- [Test Scenarios](#-test-scenarios) - Forskellige test cases

---

## Oversigt over Systemet

**Komponenter:**
1. **Indexer** - Crawler og indexerer dokumenter til SQLite database
2. **DatabaseSplitter** - Splitter database i shards til Y-skalering (valgfrit)
3. **SearchAPI** - REST API til søgning (med auto-detection af shards)
4. **LoadBalancer** - Distribuerer requests til multiple SearchAPI instances
5. **ConsoleSearch** - Kommandolinje søge-interface (deprecated, brug SearchAPI)

**Arkitektur Flow:**
```
Data Files → Indexer → Database → DatabaseSplitter → Shards
                                         ↓
                                    SearchAPI (queries shards)
                                         ↓
                                   LoadBalancer (distribuerer load)
                                         ↓
                                      Client
```

---

## 🚀 Quick Start (Basis Setup)

### Trin 1: Indexer Dokumenter

```bash
cd SearchEngine/indexer
dotnet run
```

**Prompt:** Vælg datasæt størrelse:
- `small` - Få tusind dokumenter (hurtig test)
- `medium` - ~600k dokumenter (anbefalet til test)
- `large` - Millioner af dokumenter (fuld dataset)

**Output:** `Data/searchDBmedium.db` (eller searchDBsmall/large.db)

**Hvor lang tid?**
- small: ~30 sekunder
- medium: ~5-10 minutter
- large: ~30-60 minutter

---

### Trin 2: Start SearchAPI

```bash
cd SearchEngine/SearchAPI
dotnet run
```

API starter på: `http://localhost:5281`

**Test det:**
```bash
curl "http://localhost:5281/api/search?query=meeting&maxResults=10"
```

**Forventet output:**
```json
{
  "results": [
    {
      "docId": 12345,
      "url": "/path/to/document.txt",
      "score": 42
    },
    ...
  ],
  "totalResults": 1523,
  "queryTime": "45ms"
}
```

---

## 📊 Avanceret: Y-Skalering (Database Sharding)

Hvis du vil demonstrere **Y-skalering** (horizontal database partitioning):

### Trin 1: Indexer Data (som normalt)

```bash
cd SearchEngine/indexer
dotnet run
# Vælg "medium" → Opretter searchDBmedium.db
```

---

### Trin 2: Split Database i Shards

```bash
cd SearchEngine/DatabaseSplitter
dotnet run
```

**Prompt:** Hvor mange shards? (2-10)
- Tast `3` for 3 shards

**Output:**
```
Source database: searchDBmedium.db
Total dokumenter: 600000

Opretter 3 shards med ~200000 dokumenter hver

Opretter shard 1: searchDB_shard1.db
  Dokumenter: 1 til 200000 (200000 docs)
  ✓ Shard 1 oprettet!

Opretter shard 2: searchDB_shard2.db
  Dokumenter: 200001 til 400000 (200000 docs)
  ✓ Shard 2 oprettet!

Opretter shard 3: searchDB_shard3.db
  Dokumenter: 400001 til 600000 (200000 docs)
  ✓ Shard 3 oprettet!

✓ FÆRDIG! 3 shards oprettet
```

**Resultat:** 3 databaser i `Data/`:
- `searchDB_shard1.db` (200k docs)
- `searchDB_shard2.db` (200k docs)
- `searchDB_shard3.db` (200k docs)

---

### Trin 3: Start SearchAPI (Auto-detekterer Shards)

```bash
cd SearchEngine/SearchAPI
dotnet run
```

**Hvad sker der:**
- SearchAPI finder automatisk alle `searchDB_shard*.db` filer
- Bruger `MultiDatabaseWrapper` til at query alle shards
- Merger og sorterer resultater fra alle databaser

**Test Y-skalering:**
```bash
curl "http://localhost:5281/api/search?query=meeting&maxResults=20"
```

**Forventet:** Resultater fra alle 3 shards merged sammen, sorteret efter relevans.

**Document IDs:**
- Shard 0: IDs 1-999999
- Shard 1: IDs 1000000-1999999
- Shard 2: IDs 2000000-2999999

(Composite ID system: `shardIndex * 1000000 + originalDocId`)

---

## ⚖️ Load Balancing (Multiple API Instances)

For at demonstrere **load balancing** med flere SearchAPI instances:

### Trin 1: Start Multiple SearchAPI Instances

**Terminal 1 - API Instance 1:**
```bash
cd SearchEngine/SearchAPI
id=1 dotnet run --urls "http://localhost:5281"
```

**Terminal 2 - API Instance 2:**
```bash
cd SearchEngine/SearchAPI
id=2 dotnet run --urls "http://localhost:5282"
```

**Terminal 3 - API Instance 3:**
```bash
cd SearchEngine/SearchAPI
id=3 dotnet run --urls "http://localhost:5283"
```

---

### Trin 2: Start LoadBalancer

**Terminal 4 - LoadBalancer:**
```bash
cd SearchEngine/LoadBalancer
dotnet run
```

LoadBalancer starter på: `http://localhost:5280`

---

### Trin 3: Test Load Balancing

```bash
# Request 1 → API instance 1
curl "http://localhost:5280/api/search?query=meeting&maxResults=5"

# Request 2 → API instance 2
curl "http://localhost:5280/api/search?query=project&maxResults=5"

# Request 3 → API instance 3
curl "http://localhost:5280/api/search?query=email&maxResults=5"

# Request 4 → API instance 1 (round-robin)
curl "http://localhost:5280/api/search?query=schedule&maxResults=5"
```

**Check hvilken instance der handlede requesten:**
```bash
curl "http://localhost:5280/api/search/ping"
# Output: "1", "2", eller "3" (roterer round-robin)
```

---

## 🔥 Fuld Demo Setup (Y-Skalering + Load Balancing)

Sådan kører du hele systemet med både **Y-skalering** og **load balancing**:

### Setup (én gang):

```bash
# 1. Indexer data
cd SearchEngine/indexer && dotnet run
# Vælg "medium"

# 2. Split i shards
cd ../DatabaseSplitter && dotnet run
# Vælg "3" shards
```

### Kør Systemet (4 terminals):

**Terminal 1:**
```bash
cd SearchEngine/SearchAPI
id=1 dotnet run --urls "http://localhost:5281"
```

**Terminal 2:**
```bash
cd SearchEngine/SearchAPI
id=2 dotnet run --urls "http://localhost:5282"
```

**Terminal 3:**
```bash
cd SearchEngine/SearchAPI
id=3 dotnet run --urls "http://localhost:5283"
```

**Terminal 4:**
```bash
cd SearchEngine/LoadBalancer
dotnet run
```

### Test:

```bash
# Query gennem LoadBalancer
curl "http://localhost:5280/api/search?query=meeting+schedule&maxResults=10"

# Check hvilken API instance blev brugt
curl "http://localhost:5280/api/search/ping"

# Gentag - se load rotation
curl "http://localhost:5280/api/search/ping"
curl "http://localhost:5280/api/search/ping"
# Output: "1", "2", "3", "1", "2", "3", ...
```

**Hvad sker der:**
1. Request → LoadBalancer (port 5280)
2. LoadBalancer → Vælger én SearchAPI instance (5281, 5282, eller 5283) via round-robin
3. SearchAPI → Queries alle 3 database shards via MultiDatabaseWrapper
4. SearchAPI → Merger resultater og returnerer til LoadBalancer
5. LoadBalancer → Returnerer til klient

---

## 📝 API Endpoints

### SearchAPI

**Base URL:** `http://localhost:5281` (eller 5281/5282/5283 med load balancer)

#### `GET /api/search`

Søg efter dokumenter.

**Query Parameters:**
- `query` (required) - Søgeord (kan være flere ord separeret med mellemrum)
- `maxResults` (optional, default: 20) - Max antal resultater
- `caseSensitive` (optional, default: false) - Case-sensitive søgning
- `showFullPaths` (optional, default: false) - Vis fulde fil-paths

**Eksempel:**
```bash
curl "http://localhost:5281/api/search?query=meeting+project&maxResults=15&caseSensitive=false"
```

**Response:**
```json
{
  "results": [
    {
      "docId": 1005432,
      "url": "/allen-p/inbox/meeting_notes.txt",
      "score": 87,
      "preview": "Meeting scheduled for project review..."
    },
    ...
  ],
  "totalHits": 2341,
  "queryTime": "123ms",
  "missingWords": []
}
```

#### `GET /api/search/ping`

Health check endpoint - returnerer API instance ID.

**Eksempel:**
```bash
curl "http://localhost:5281/api/search/ping"
```

**Response:** `"1"` (eller miljøvariabel `id` hvis sat)

---

### LoadBalancer

**Base URL:** `http://localhost:5280`

#### `GET /api/search`

Samme som SearchAPI, men distribuerer requests til backend instances.

#### `GET /api/search/ping`

Returnerer ID fra én af backend API instances (roterer round-robin).

---

## 🛠️ Troubleshooting

### Problem: "Database not found"

**Løsning:** Kør indexer først:
```bash
cd SearchEngine/indexer && dotnet run
```

### Problem: "No shards found" (når du forventer shards)

**Løsning:** Kør DatabaseSplitter:
```bash
cd SearchEngine/DatabaseSplitter && dotnet run
```

### Problem: SearchAPI crasher med "Database locked"

**Årsag:** Anden process har databasen åben (fx anden SearchAPI instance eller SQLite browser)

**Løsning:** 
1. Luk alle SearchAPI instances
2. Luk SQLite browser/viewer værktøjer
3. Genstart SearchAPI

### Problem: LoadBalancer returnerer "Connection refused"

**Årsag:** SearchAPI instances ikke startet eller forkerte ports

**Løsning:** 
1. Verificer SearchAPI instances kører:
```bash
curl http://localhost:5281/api/search/ping
curl http://localhost:5282/api/search/ping
curl http://localhost:5283/api/search/ping
```
2. Check LoadBalancer konfiguration i `LoadBalancer/Program.cs`

### Problem: Queries returnerer duplikerede resultater

**Årsag:** Du har kopieret samme database til alle shards (ikke splittet korrekt)

**Løsning:** Brug DatabaseSplitter til at lave ægte split:
```bash
cd SearchEngine/DatabaseSplitter && dotnet run
```

---

## 📚 Arkitektur-Koncepter

### Y-Skalering (Horizontal Partitioning)
- **Formål:** Fordel data arbitrært over flere databaser
- **Implementering:** `MultiDatabaseWrapper` queries alle shards og merger resultater
- **Trade-off:** Ikke hurtigere søgning, men bedre skalerbarhed
- **Brug:** Når én database bliver for stor

### Z-Skalering (Logical Partitioning)
- **Formål:** Opdel data baseret på attributter (geografi, kunde, tid)
- **Eksempel:** EU-data i DB1, US-data i DB2
- **Trade-off:** Hurtigere søgning (kun query relevante shards)
- **Status:** Ikke implementeret (kun Y-skalering er implementeret)

### Load Balancing
- **Formål:** Distribuér requests over flere API instances
- **Strategi:** Round-robin (1→2→3→1→2→3...)
- **Fordel:** Højere throughput, bedre resource udnyttelse
- **Brug:** Når én API instance ikke kan håndtere load

---

## 🎯 Test-Scenarios

### Scenario 1: Basis Søgning (Ingen Skalering)

```bash
# 1. Indexer
cd SearchEngine/indexer && dotnet run

# 2. Start API
cd ../SearchAPI && dotnet run

# 3. Test
curl "http://localhost:5281/api/search?query=meeting&maxResults=10"
```

**Demonstrerer:** Basis funktionalitet

---

### Scenario 2: Y-Skalering (Database Sharding)

```bash
# 1. Indexer
cd SearchEngine/indexer && dotnet run

# 2. Split
cd ../DatabaseSplitter && dotnet run

# 3. Start API (auto-detekterer shards)
cd ../SearchAPI && dotnet run

# 4. Test
curl "http://localhost:5281/api/search?query=meeting&maxResults=10"
```

**Demonstrerer:** Horizontal partitioning, multi-database queries, result merging

---

### Scenario 3: Load Balancing (Ingen Shards)

```bash
# 1. Indexer
cd SearchEngine/indexer && dotnet run

# 2. Start 3 API instances (3 terminals)
id=1 dotnet run --urls "http://localhost:5281"
id=2 dotnet run --urls "http://localhost:5282"
id=3 dotnet run --urls "http://localhost:5283"

# 3. Start LoadBalancer (4. terminal)
cd ../LoadBalancer && dotnet run

# 4. Test
curl "http://localhost:5280/api/search/ping"  # Se rotation
```

**Demonstrerer:** Request distribution, round-robin load balancing

---

### Scenario 4: Fuld Skalering (Y-Skalering + Load Balancing)

```bash
# 1. Indexer + Split
cd SearchEngine/indexer && dotnet run
cd ../DatabaseSplitter && dotnet run

# 2. Start 3 API instances med shards (3 terminals)
cd ../SearchAPI
id=1 dotnet run --urls "http://localhost:5281"
id=2 dotnet run --urls "http://localhost:5282"
id=3 dotnet run --urls "http://localhost:5283"

# 3. Start LoadBalancer (4. terminal)
cd ../LoadBalancer && dotnet run

# 4. Test
curl "http://localhost:5280/api/search?query=meeting+schedule&maxResults=20"
```

**Demonstrerer:** Fuld distribueret arkitektur med både data-partitioning og load distribution

---

## 📊 Performance Forventninger

### Single API (Ingen Shards)
- Query tid: ~50-200ms (medium dataset)
- Throughput: ~50-100 requests/sekund

### Single API (3 Shards)
- Query tid: ~150-400ms (3x slowere - queries alle shards)
- Throughput: ~20-50 requests/sekund
- **Fordel:** Kan håndtere større datasets (skalerbarhed)

### Load Balanced (3 Instances, Ingen Shards)
- Query tid: ~50-200ms per request
- Throughput: ~150-300 requests/sekund (3x bedre)
- **Fordel:** Højere concurrent load

### Load Balanced (3 Instances, 3 Shards hver)
- Query tid: ~150-400ms per request
- Throughput: ~60-150 requests/sekund
- **Fordel:** Både høj throughput og kan håndtere meget store datasets

---

**Læs mere:**
- `Y-Skalering-Dokumentation.md` - Detaljeret arkitektur og implementation
- `DatabaseSplitter/README.md` - Database split værktøj dokumentation
- `SearchAPI/README.md` - API dokumentation (hvis den findes)
