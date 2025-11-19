#!/bin/bash

echo "🚀 Deploying SearchEngine to Kubernetes (Minikube)"
echo "=================================================="
echo ""

# Check if minikube is running
if ! minikube status &> /dev/null; then
    echo "❌ Minikube is not running!"
    echo "📋 Start minikube with: minikube start"
    exit 1
fi

echo "✅ Minikube is running"
echo ""

# Use minikube's Docker daemon
echo "🔧 Configuring Docker to use Minikube..."
eval $(minikube docker-env)

echo ""
echo "🔨 Building Docker images in Minikube..."

# Build images
echo "  - Building SearchAPI..."
docker build -t searchengine-searchapi:latest -f SearchAPI/Dockerfile .

echo "  - Building LoadBalancer..."
docker build -t searchengine-loadbalancer:latest -f LoadBalancer/Dockerfile .

echo "  - Building SearchWebApp..."
docker build -t searchengine-webapp:latest -f SearchWebApp/Dockerfile .

echo ""
echo "✅ Images built successfully"

# Setup PersistentVolume with data
echo ""
echo "�️  Setting up PersistentVolume with database shards..."
./setup-data-pv.sh

echo ""
echo "🚀 Deploying to Kubernetes..."

# Apply deployments
kubectl apply -f k8s/searchapi-deployment.yaml
kubectl apply -f k8s/loadbalancer-deployment.yaml
kubectl apply -f k8s/webapp-deployment.yaml

echo ""
echo "⏳ Waiting for pods to be ready..."
kubectl wait --for=condition=ready pod -l app=searchapi --timeout=120s
kubectl wait --for=condition=ready pod -l app=loadbalancer --timeout=60s
kubectl wait --for=condition=ready pod -l app=searchwebapp --timeout=60s

echo ""
echo "✅ All pods are ready!"
echo ""
echo "📊 Services:"
echo "   WebApp:       http://$(minikube ip):30000"
echo "   LoadBalancer: http://$(minikube ip):30280"
echo ""
echo "📋 Useful commands:"
echo "   View pods:     kubectl get pods"
echo "   View services: kubectl get services"
echo "   Logs:          kubectl logs -l app=searchapi"
echo "   Dashboard:     minikube dashboard"
echo "   Stop mount:    kill $MOUNT_PID"
echo "   Delete all:    kubectl delete -f k8s/"
echo ""
echo "Happy searching! 🔍"
