#!/bin/bash

# Funkce pro kontrolu, zda je Docker spuštěný
check_docker() {
    if ! docker info > /dev/null 2>&1; then
        echo "❌ Docker není spuštěn. Prosím, spusťte Docker a zkuste to znovu."
        exit 1
    fi
}

# Funkce pro čekání na dostupnost služby
wait_for_service() {
    local host=$1
    local port=$2
    local service_name=$3
    local max_attempts=30
    local attempt=1

    echo "⏳ Čekám na dostupnost služby $service_name na $host:$port..."
    
    while ! nc -z $host $port >/dev/null 2>&1; do
        if [ $attempt -eq $max_attempts ]; then
            echo "❌ Služba $service_name není dostupná po $max_attempts pokusech."
            return 1
        fi
        echo -n "."
        sleep 2
        ((attempt++))
    done
    echo "✅ Služba $service_name je připravena!"
    return 0
}

# Funkce pro kontrolu health endpointu
check_health() {
    local max_attempts=30
    local attempt=1
    local url="http://localhost:$API_PORT/health"

    echo "🏥 Kontroluji health endpoint..."
    
    while [ $attempt -le $max_attempts ]; do
        response=$(curl -s -w "\\n%{http_code}" "$url")
        status_code=$(echo "$response" | tail -n1)
        body=$(echo "$response" | sed '$d')

        if [ "$status_code" = "200" ]; then
            echo "✅ Health endpoint je dostupný!"
            echo "Response: $body"
            return 0
        fi

        echo -n "."
        sleep 2
        ((attempt++))
    done

    echo "❌ Health endpoint není dostupný po $max_attempts pokusech."
    return 1
}

# Kontrola Docker
check_docker

# Zastavit běžící kontejnery a vyčistit
echo "🔄 Zastavuji běžící kontejnery..."
docker-compose down
docker system prune -f

# Stáhnout potřebné Docker images
echo "📥 Stahuji potřebné Docker images..."
docker pull postgres:14

# Nastavit proměnné prostředí
export API_PORT=8080
export ASPNETCORE_ENVIRONMENT=Development

# Sestavit a spustit aplikaci
echo "🚀 Spouštím aplikaci..."
docker-compose up --build -d

# Čekání na dostupnost služeb
wait_for_service localhost 5432 "PostgreSQL" || exit 1
wait_for_service localhost $API_PORT "API" || exit 1
check_health || exit 1

echo "
🎉 Aplikace je úspěšně nasazena!
📊 GraphQL Playground: http://localhost:$API_PORT/graphql
🔍 Sledování logů: docker-compose logs -f
🏥 Health endpoint: http://localhost:$API_PORT/health

Pro zobrazení logů použijte:
docker-compose logs -f
"

# Zobrazit logy
docker-compose logs -f
