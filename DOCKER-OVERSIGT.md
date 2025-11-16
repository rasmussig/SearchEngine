# Docker Oversigt

## Filer

**Dockerfiles (i hver projekt-mappe):**
- `SearchAPI/Dockerfile`
- `LoadBalancer/Dockerfile`
- `SearchWebApp/Dockerfile`

**Konfiguration:**
- `docker-compose.yml` - Orchestrering (i root)
- `LoadBalancer/appsettings.json` - Lokal udvikling
- `LoadBalancer/appsettings.Docker.json` - Docker miljø
- `.dockerignore` - Build optimization (i hver mappe)

**Scripts:**
- `start-docker.bat` / `start-docker.sh`

---

## 🚀 Hvordan bruges det?

### TRIN 1: Forbered Data (éngangs-setup - UDEN Docker)
```bash
# Indexer dokumenter (kør lokalt, IKKE i container)
cd indexer
dotnet run
# Vælg "medium"

# Valgfrit: Split til shards for Y-skalering (kør lokalt)
cd ../DatabaseSplitter
dotnet run
# Vælg "3"

# Database er nu klar i Data/ mappen
```

**Hvorfor ikke containerisere Indexer?**
- 🚫 Indexer er en one-time data preparation tool
- 🚫 Ikke en runtime service med API endpoints
- 🚫 Behøver ikke skalering eller continuous deployment
- ✅ Nemmere at køre direkte med `dotnet run`

### TRIN 2: Start Containers

**Windows:**
```cmd
start-docker.bat
```

**Linux/Mac:**
```bash
chmod +x start-docker.sh
./start-docker.sh
```

**Eller manuelt:**
```bash
docker-compose up --build -d
```

### TRIN 3: Test

**Åbn browser:**
```
http://localhost:5000
```

**Test LoadBalancer:**
```bash
curl "http://localhost:5280/api/search/status"
curl "http://localhost:5280/api/search?query=meeting&maxResults=10"
```

---

## Ports

| Service      | Port  |
|--------------|-------|
| WebApp       | 5000  |
| LoadBalancer | 5280  |
| SearchAPI #1 | 5281  |
| SearchAPI #2 | 5282  |
| SearchAPI #3 | 5283  |

---

## Commands

```bash
docker-compose up -d         # Start
docker-compose down          # Stop
docker-compose logs -f       # View logs
docker-compose ps            # Status
```
