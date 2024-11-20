#!/bin/bash

# Zastavení běžících kontejnerů
docker-compose down

# Sestavení a spuštění kontejnerů
docker-compose up --build -d

# Čekání na dostupnost databáze
echo "Čekání na inicializaci databáze..."
sleep 10

echo "Aplikace je dostupná na http://localhost:5000" 