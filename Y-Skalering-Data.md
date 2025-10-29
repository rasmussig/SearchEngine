# Y-Skalering af Database - Opgave 7.1

## Hvad er Y-Skalering vs Z-Skalering?

### Y-Skalering (Horizontal Partitioning)
- **Arbitrær opdeling** af data på tværs af flere databaser
- Hver database indeholder en **tilfældig delmængde** af alle rækker
- Eksempel: 600.000 dokumenter fordelt som 200k + 200k + 200k
- **Ingen logik** i hvordan data splittes - bare en mekanisk opdeling

### Z-Skalering (Logical Partitioning) 
- **Attribut-baseret** opdeling af data
- Hver database indeholder data baseret på **specifikke kriterier**
- Eksempel: Kunde A's data i DB1, Kunde B's data i DB2
- Eksempel: EU-data i DB1, US-data i DB2, ASIA-data i DB3
- **Logisk opdeling** baseret på forretningsregler

**Forskellen:** Y = tilfældig split, Z = logisk split baseret på attributter (geografi, kunde, tid, osv.)

---

## Arkitektoniske Løsningsforslag

### Løsning 1: MultiDatabaseWrapper Pattern (VALGT)

**Beskrivelse:**
- En wrapper-klasse der implementerer `IDatabase` interface
- Indeholder en liste af `DatabaseSqlite` objekter (én per shard)
- Queries sendes til **alle shards**, resultater merges og sorteres
- Bruger composite document IDs: `shardIndex * 1000000 + originalDocId`

**Fordele:**
- ✅ Ingen ændringer til `SearchLogic` - respekterer dependency injection
- ✅ Auto-detection: Finder automatisk shards ved opstart
- ✅ Transparent: `SearchController` behøver ikke kende til multiple databases
- ✅ Nem at teste: Tilføj/fjern shard-filer, genstart API

**Ulemper:**
- ❌ Query sendes til alle shards (kan være slow med mange shards)
- ❌ Merge-overhead: Sorterer resultater fra alle databaser

---

### Løsning 2: Smart Router Pattern

**Beskrivelse:**
- En routing-komponent der bestemmer hvilken shard der skal queries
- Kunne bruge hashing (f.eks. `wordHash % numberOfShards`) til at vælge shard
- Queries sendes kun til **relevante shards**

**Fordele:**
- ✅ Reducerer antal queries (kun query relevante shards)
- ✅ Bedre performance ved mange shards
- ✅ Kan optimere routing baseret på data-distribution

**Ulemper:**
- ❌ Kræver metadata: Skal vide hvilke ord/dokumenter er i hvilken shard
- ❌ Kompleks implementering: Routing-logik, metadata-vedligeholdelse
- ❌ Ikke passende for arbitrær Y-skalering (hvor data er tilfældigt fordelt)

**Hvorfor ikke valgt:** For kompleks til simpel Y-skalering, hvor data er arbitrært splittet.

---

## Implementeret Løsning

### Ændrede Filer

**1. `SearchAPI/Logic/MultiDatabaseWrapper.cs` (NY)**
- Implementerer `IDatabase` interface
- Queries alle shards parallelt
- Merger og sorterer resultater baseret på hit count
- Håndterer composite IDs: `shardIndex * 1000000 + originalDocId`

**2. `Shared/Paths.cs`**
- Tilføjet `GetDatabaseShards()` metode
- Finder alle `searchDB_shard*.db` filer i Data-mappen
- Returnerer sorteret liste af shard-paths

**3. `SearchAPI/Logic/DatabaseSqlite.cs`**
- Tilføjet overloaded constructor: `DatabaseSqlite(string databasePath)`
- Muliggør instantiering med custom database path

**4. `SearchAPI/Controllers/SearchController.cs`**
- Auto-detection af shards ved opstart
- Hvis shards findes: Brug `MultiDatabaseWrapper`
- Hvis ingen shards: Brug standard `DatabaseSqlite`

### Composite ID System

Dokumenter får et **composite ID** når de hentes fra shards:

```
compositeId = shardIndex * 1000000 + originalDocId

Eksempel:
- Document ID 5432 i shard 0 → Composite ID = 5432
- Document ID 5432 i shard 1 → Composite ID = 1005432
- Document ID 5432 i shard 2 → Composite ID = 2005432
```

Dette sikrer unikke IDs på tværs af shards.

---

## Test-Vejledning

### 1. Indexer Dataene (Hvis du ikke har gjort det)

```bash
cd SearchEngine/indexer
dotnet run
# Vælg "medium" (eller "large"/"small")
```

Dette opretter `Data/searchDBmedium.db` med ALLE dokumenter indexeret.

### 2. Split Databasen i Shards

Brug DatabaseSplitter værktøjet til automatisk at splitte databasen:

```bash
cd SearchEngine/DatabaseSplitter
dotnet run
```

Programmet vil:
1. Finde din indexerede database (fx `searchDBmedium.db`)
2. Tælle antal dokumenter (fx 600.000)
3. Spørge hvor mange shards du vil have (standard: 3)
4. Automatisk oprette shards med lige fordeling:
   - `searchDB_shard1.db` (Doc ID 1-200000)
   - `searchDB_shard2.db` (Doc ID 200001-400000)
   - `searchDB_shard3.db` (Doc ID 400001-600000)

**Eksempel output:**
```
=== Database Splitter til Y-Skalering ===

Source database: C:\...\Data\searchDBmedium.db
Total dokumenter: 600000

Hvor mange shards vil du oprette? (2-10): 3

Opretter 3 shards med ~200000 dokumenter hver

Opretter shard 1: searchDB_shard1.db
  Dokumenter: 1 til 200000 (200000 docs)
  ✓ Shard 1 oprettet!
...
✓ FÆRDIG! 3 shards oprettet
```

### Alternativ: Quick Test (med duplikater)

Hvis du bare vil teste funktionaliteten hurtigt:

```bash
cd Data
cp searchDBmedium.db searchDB_shard1.db
cp searchDBmedium.db searchDB_shard2.db
cp searchDBmedium.db searchDB_shard3.db
```

⚠️ **OBS:** Dette giver 3 identiske databaser → duplikerede resultater. Kun til funktionstest!

### 2. Start SearchAPI

```bash
cd SearchEngine/SearchAPI
dotnet run
```

API vil automatisk detektere de 3 shards ved opstart.

### 3. Test Queries

```bash
# Basic search
curl "http://localhost:5147/api/search?query=meeting&maxResults=10"

# Multiple words
curl "http://localhost:5147/api/search?query=project+schedule&maxResults=20"

# Case sensitive
curl "http://localhost:5147/api/search?query=Enron&maxResults=5&caseSensitive=true"
```

**Forventet resultat:** 
- Resultater fra alle 3 shards merget sammen
- Sorteret efter relevans (hit count)
- Document IDs vil være composite (0-999999, 1000000-1999999, 2000000-2999999)

### 4. Verificer Merged Results

Check at document IDs kommer fra forskellige shards:
- IDs 0-999999 = Shard 0
- IDs 1000000-1999999 = Shard 1  
- IDs 2000000-2999999 = Shard 2

---

## Performance-Overvejelser

- **Query tid:** Stiger lineært med antal shards (3x shards = ~3x query tid)
- **Merge overhead:** Minimal for få resultater (<100 docs)
- **Memory:** Alle shard-connections holdes åbne (3 connections)

**Skalering:** Løsningen fungerer godt op til ~5-10 shards. 
Ved flere shards bør man overveje:
- Caching af query-resultater
- Async/parallel queries
- Smart routing til at undgå at query alle shards

---

## Konklusion

Y-skalering er implementeret med **MultiDatabaseWrapper pattern**, som giver:
- ✅ Transparent integration (ingen ændringer til forretningslogik)
- ✅ Auto-detection af shards
- ✅ Simpel at teste (kør DatabaseSplitter værktøj)
- ✅ Merge af resultater fra 3 databases med 200k dokumenter hver

**DatabaseSplitter værktøj** automatiserer processen:
- Tager din eksisterende indexerede database
- Splitter den i N shards med lige fordeling
- Kopierer dokumenter, ord og occurrences korrekt
- Klar til brug med SearchAPI

Løsningen er optimal til små-mellemstore antal shards og arbitrær data-opdeling.

---

## Quick Start Guide

```bash
# 1. Indexer data (hvis ikke gjort)
cd SearchEngine/indexer && dotnet run

# 2. Split database i shards
cd ../DatabaseSplitter && dotnet run

# 3. Start SearchAPI (auto-detekterer shards)
cd ../SearchAPI && dotnet run

# 4. Test search
curl "http://localhost:5147/api/search?query=meeting&maxResults=10"
```
