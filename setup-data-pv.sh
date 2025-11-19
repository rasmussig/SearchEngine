#!/bin/bash

echo "🗄️  Kopierer database shards til Kubernetes PersistentVolume..."

# Step 1: Opret PersistentVolume og PersistentVolumeClaim
echo "📦 Opretter PersistentVolume..."
kubectl apply -f k8s/data-pv.yaml

# Vent på at PVC bliver bound
echo "⏳ Venter på at PVC bliver bound..."
kubectl wait --for=jsonpath='{.status.phase}'=Bound pvc/searchengine-data-pvc --timeout=30s || true

# Step 2: Kopier database filer til Minikube VM's /data folder
echo "📂 Kopierer database filer til Minikube VM..."

# Find absolut path til Data folderen
SCRIPT_DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )"
DATA_PATH="$SCRIPT_DIR/../Data"

if [ ! -d "$DATA_PATH" ]; then
    echo "❌ Kan ikke finde Data folder på: $DATA_PATH"
    exit 1
fi

# Opret en midlertidig pod til at kopiere data ind med
echo "🚀 Opretter midlertidig data-copy pod..."
cat <<EOF | kubectl apply -f -
apiVersion: v1
kind: Pod
metadata:
  name: data-copy-pod
spec:
  containers:
  - name: alpine
    image: alpine:latest
    command: ["sleep", "3600"]
    volumeMounts:
    - name: data-volume
      mountPath: /data
  volumes:
  - name: data-volume
    hostPath:
      path: /data
      type: DirectoryOrCreate
EOF

# Vent på at pod er klar
echo "⏳ Venter på at pod er klar..."
kubectl wait --for=condition=ready pod/data-copy-pod --timeout=60s

# Kopier hver shard fil
echo "📋 Kopierer shard filer..."
kubectl cp "$DATA_PATH/searchDB_shard1.db" data-copy-pod:/data/searchDB_shard1.db
kubectl cp "$DATA_PATH/searchDB_shard2.db" data-copy-pod:/data/searchDB_shard2.db
kubectl cp "$DATA_PATH/searchDB_shard3.db" data-copy-pod:/data/searchDB_shard3.db

# Verificer at filerne er kopieret
echo "✅ Verificerer filer..."
kubectl exec data-copy-pod -- ls -lh /data/

# Cleanup midlertidig pod
echo "🧹 Rydder op..."
kubectl delete pod data-copy-pod

echo ""
echo "✅ Database shards kopieret til PersistentVolume!"
echo "   Nu kan du deploye SearchAPI pods med: kubectl apply -f k8s/searchapi-deployment.yaml"
