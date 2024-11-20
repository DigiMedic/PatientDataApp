#!/bin/bash

# Zastavit běžící kontejnery
docker-compose down

# Vyčistit všechny images
docker-compose down --rmi all

# Vyčistit systém
docker system prune -f

# Nastavit port
export API_PORT=5001

# Spustit aplikaci
docker-compose up --build -d

# Výpis logů
echo "Aplikace se spouští..."
echo "GraphQL Playground bude dostupný na: http://localhost:${API_PORT}/graphql"
echo "Čekám na nastartování aplikace..."
sleep 5  # Počkáme 5 sekund na nastartování
echo "Aplikace by měla být připravena"
docker-compose logs -f
