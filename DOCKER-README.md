# Docker Guide - SearchEngine

## Komponenter

**Containeriseres:**
- SearchAPI (3 instanser) - ports 5281-5283
- LoadBalancer - port 5280
- SearchWebApp - port 5000

**Køres lokalt (ikke containeriseret):**
- Indexer - data preparation
- DatabaseSplitter - shard splitting

---

## Quick Start

### 1. Forbered Data
```bash
cd indexer && dotnet run    # Vælg "medium"
cd DatabaseSplitter && dotnet run  # Vælg "3" (valgfrit)
```

### 2. Start Containers
```bash
docker-compose up -d --build
```

### 3. Test
- WebApp: http://localhost:5000
- LoadBalancer: http://localhost:5280/api/search/status

---

## 🛠️ Docker Commands

### Start services:
```bash
docker-compose up          # Start med logs
docker-compose up -d       # Start i baggrunden
```

### Stop services:
```bash
docker-compose down        # Stop og fjern containers
docker-compose stop        # Stop containers (bevar state)
```

### Rebuild specific service:
```bash
docker-compose up --build searchapi1
docker-compose up --build loadbalancer
docker-compose up --build webapp
```

### View logs:
```bash
docker-compose logs -f                 # Alle services
docker-compose logs -f searchapi1      # Specifik service
docker-compose logs -f loadbalancer
```

### Restart specific service:
```bash
docker-compose restart searchapi1
docker-compose restart loadbalancer
```

### Clean up (fjern alt):
```bash
docker-compose down --volumes --rmi all
```

---

## Konfiguration

LoadBalancer bruger forskellige URLs afhængig af miljø:
- **Lokal:** `http://localhost:5281/5282/5283` (appsettings.json)
- **Docker:** `http://searchapi1:8080/...` (appsettings.Docker.json)

Database mountes read-only i SearchAPI containers:
```yaml
volumes:
  - ./Data:/app/Data:ro
```

---

## Troubleshooting

**LoadBalancer kan ikke finde SearchAPI:**
```bash
# Check containers kører
docker-compose ps

# Check logs
docker-compose logs -f loadbalancer
```

**Database ikke fundet:**
```bash
# Kør indexer først
cd indexer && dotnet run

# Verificer Data/ eksisterer
ls Data/
```

**Port i brug:**
```bash
# Find process (Windows)
netstat -ano | findstr :5000
taskkill /PID <pid> /F
```
