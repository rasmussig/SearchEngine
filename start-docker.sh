#!/bin/bash

# Quick Start Script - SearchEngine Docker
echo "🚀 SearchEngine Docker Quick Start"
echo "=================================="
echo ""

# Check if Data directory exists
if [ ! -d "../Data" ]; then
    echo "❌ Data directory not found!"
    echo "📋 Please run indexer first:"
    echo "   cd indexer && dotnet run"
    exit 1
fi

# Check if database exists
if [ ! -f "../Data/searchDBmedium.db" ] && [ ! -f "../Data/searchDB_shard1.db" ]; then
    echo "❌ No database found in Data/"
    echo "📋 Please run indexer first:"
    echo "   cd indexer && dotnet run"
    exit 1
fi

echo "✅ Database found"
echo ""
echo "🔨 Building Docker images..."
docker-compose build

if [ $? -ne 0 ]; then
    echo "❌ Build failed!"
    exit 1
fi

echo ""
echo "✅ Build successful"
echo ""
echo "🚀 Starting containers..."
docker-compose up -d

if [ $? -ne 0 ]; then
    echo "❌ Failed to start containers!"
    exit 1
fi

echo ""
echo "✅ All containers started!"
echo ""
echo "📊 Service URLs:"
echo "   WebApp:       http://localhost:5000"
echo "   LoadBalancer: http://localhost:5280"
echo "   SearchAPI 1:  http://localhost:5281"
echo "   SearchAPI 2:  http://localhost:5282"
echo "   SearchAPI 3:  http://localhost:5283"
echo ""
echo "📋 Useful commands:"
echo "   View logs:    docker-compose logs -f"
echo "   Stop:         docker-compose down"
echo "   Restart:      docker-compose restart"
echo ""
echo "🎯 Testing LoadBalancer status:"
sleep 5
curl -s "http://localhost:5280/api/search/status" | python -m json.tool || echo "LoadBalancer not ready yet, wait a few seconds..."
echo ""
echo "Happy searching! 🔍"
