# DatabaseSplitter

Værktøj til at splitte en indexeret database i flere shards til Y-skalering.

## Formål

Tager en stor indexeret database (fx `searchDBmedium.db` med 600k dokumenter) og splitter den i flere mindre databaser (shards) med lige fordeling af dokumenter.

## Brug

```bash
cd SearchEngine/DatabaseSplitter
dotnet run
```

Programmet vil:
1. Finde din indexerede database via `Paths.DATABASE`
2. Vise antal dokumenter
3. Spørge hvor mange shards du vil oprette (2-10)
4. Oprette shards med navnekonvention: `searchDB_shard1.db`, `searchDB_shard2.db`, osv.

## Eksempel

```
Source database: C:\...\Data\searchDBmedium.db
Total dokumenter: 600000

Hvor mange shards vil du oprette? (2-10): 3

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

## Hvad Kopieres?

For hver shard kopieres:
- **Documents**: Dokumenter i det tildelte ID-range
- **Words**: Kun ord der bruges i disse dokumenter
- **Occurrences**: Alle forekomster af ord i disse dokumenter

Dette sikrer at hver shard er fuldt selvstændig og kan queires uafhængigt.

## Efter Splitting

Start SearchAPI - den vil automatisk detektere shards:

```bash
cd SearchEngine/SearchAPI
dotnet run
```

SearchAPI vil finde alle `searchDB_shard*.db` filer og bruge `MultiDatabaseWrapper` til at query dem alle.
