# Kubernetes Deployment Guide

## 📦 Oversigt

Kør SearchEngine i Kubernetes/Minikube med:
- **3 SearchAPI pods** (auto-load balanced af Kubernetes)
- **1 LoadBalancer pod**
- **1 SearchWebApp pod**

---

## 🚀 Quick Start

### 1. Start Minikube
```bash
minikube start
```

### 2. Deploy til Kubernetes
```bash
chmod +x deploy-k8s.sh
./deploy-k8s.sh
```

### 3. Åbn WebApp
```bash
# Find URL
minikube service searchwebapp --url

# Eller åbn direkte
minikube service searchwebapp
```

---

## 📋 Manuel Deployment

Hvis du vil gøre det manuelt:

### 1. Brug Minikube Docker
```bash
eval $(minikube docker-env)
```

### 2. Byg Images
```bash
docker build -t searchengine-searchapi:latest -f SearchAPI/Dockerfile .
docker build -t searchengine-loadbalancer:latest -f LoadBalancer/Dockerfile .
docker build -t searchengine-webapp:latest -f SearchWebApp/Dockerfile .
```

### 3. Mount Data Folder
```bash
# I ny terminal (kører i forgrunden)
minikube mount ../Data:/data
```

### 4. Deploy
```bash
kubectl apply -f k8s/searchapi-deployment.yaml
kubectl apply -f k8s/loadbalancer-deployment.yaml
kubectl apply -f k8s/webapp-deployment.yaml
```

### 5. Tjek Status
```bash
kubectl get pods
kubectl get services
```

---

## 🔍 Kubernetes Kommandoer

### Se pods
```bash
kubectl get pods
kubectl get pods -o wide  # Med mere info
```

### Se services
```bash
kubectl get services
```

### Logs
```bash
kubectl logs -l app=searchapi        # Alle SearchAPI pods
kubectl logs <pod-name>              # Specifik pod
kubectl logs -f <pod-name>           # Follow logs
```

### Scale SearchAPI
```bash
kubectl scale deployment searchapi --replicas=5
```

### Delete alt
```bash
kubectl delete -f k8s/
```

### Dashboard
```bash
minikube dashboard
```

---

## 📊 Services

| Service       | Type      | Port  | NodePort | URL                          |
|---------------|-----------|-------|----------|------------------------------|
| searchapi     | ClusterIP | 8080  | -        | Internal only                |
| loadbalancer  | NodePort  | 8080  | 30280    | http://minikube-ip:30280     |
| searchwebapp  | NodePort  | 8080  | 30000    | http://minikube-ip:30000     |

### Find Minikube IP
```bash
minikube ip
```

### Åbn Services
```bash
minikube service searchwebapp
minikube service loadbalancer
```

---

## 🎯 Forskelle fra Docker Compose

### Docker Compose:
- 3 separate SearchAPI containers (searchapi1, searchapi2, searchapi3)
- LoadBalancer kalder hver container direkte
- Manuel load balancing i LoadBalancer

### Kubernetes:
- 1 SearchAPI Deployment med 3 replicas (pods)
- Kubernetes Service load balancer automatisk mellem pods
- LoadBalancer kalder bare `http://searchapi:8080`
- Kubernetes håndterer distribution

### Fordele ved Kubernetes:
- ✅ Auto-scaling: `kubectl scale`
- ✅ Self-healing: Auto-restart ved crash
- ✅ Rolling updates: Zero downtime deployment
- ✅ Built-in load balancing
- ✅ Health checks og readiness probes

---

## 🐛 Troubleshooting

### Pods starter ikke
```bash
kubectl describe pod <pod-name>
kubectl logs <pod-name>
```

### Image ikke fundet
```bash
# Husk at bruge Minikube Docker
eval $(minikube docker-env)

# Rebuild images
docker build -t searchengine-searchapi:latest -f SearchAPI/Dockerfile .
```

### Data ikke mountet
```bash
# Start mount i separat terminal
minikube mount ../Data:/data

# Eller brug SSH
minikube ssh
ls /data
```

### Service ikke tilgængelig
```bash
# Find URL
minikube service searchwebapp --url

# Eller port-forward
kubectl port-forward service/searchwebapp 8080:8080
```

---

## 🧹 Cleanup

```bash
# Stop alt
kubectl delete -f k8s/

# Stop minikube
minikube stop

# Slet minikube cluster
minikube delete
```
