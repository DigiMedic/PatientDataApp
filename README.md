# PatientDataApp - Systém pro správu pacientů a MRI snímků

## Popis projektu
PatientDataApp je .NET 8 aplikace pro správu pacientů a jejich MRI snímků. Primárně využívá GraphQL API pro dotazování a manipulaci s daty, s dodatečným REST API pro zpracování DICOM souborů.

## Klíčové funkce
- Správa pacientů a jejich zdravotních záznamů
- Komplexní správa MRI snímků:
  - Podpora DICOM formátu s automatickou extrakcí metadat
  - Automatická konverze DICOM na JPEG pro náhledy
  - Tagování snímků a metadata management
  - Pokročilé vyhledávání pomocí GraphQL filtrů
- Hybridní API architektura:
  - GraphQL pro dotazování a manipulaci s daty
  - REST endpointy pro upload a download DICOM souborů
- PostgreSQL s JSONB pro flexibilní ukládání metadat

## Technologie a závislosti
- .NET 8.0
- PostgreSQL 14+ s JSONB podporou
- Entity Framework Core 8
- HotChocolate pro GraphQL
- fo-dicom pro DICOM zpracování
- Npgsql pro PostgreSQL

## Instalace

1. Klonování repozitáře:
```bash
git clone [URL_repozitáře]
cd PatientDataApp
```

2. Instalace NuGet balíčků:
```bash
dotnet restore
```

3. Konfigurace databáze v `appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=patientdb;Username=postgres;Password=postgres;Maximum Pool Size=100;Timeout=30;Command Timeout=30;"
  },
  "DatabaseSettings": {
    "MaxRetryCount": 3,
    "CommandTimeout": 30,
    "EnableDetailedErrors": true
  }
}
```

4. Vytvoření a aplikace migrací:
```bash
dotnet ef migrations add InitialCreate
dotnet ef database update
```

5. Spuštění aplikace:
```bash
dotnet run
```

## Nasazení Databáze

### Automatizované nasazení pomocí Docker Compose

1. Ujistěte se, že máte nainstalovaný Docker a Docker Compose.

2. Spusťte skript pro nasazení:
```bash
chmod +x deploy.sh
./deploy.sh
```

Tento skript automaticky sestaví a spustí kontejnery pro aplikaci a databázi. Databáze bude inicializována pomocí skriptů v `init-scripts`.

### Manuální nasazení

1. Sestavení a spuštění kontejnerů:
```bash
docker-compose up --build -d
```

2. Zastavení aplikace:
```bash
docker-compose down
```

### Přístup k logům
```bash
# Logy aplikace
docker-compose logs app

# Logy databáze
docker-compose logs db
```

### Správa dat
Data PostgreSQL jsou perzistentní díky Docker volume `postgres-data`.
Pro úplné vyčištění včetně dat použijte:
```bash
docker-compose down -v
```

## API Dokumentace

### GraphQL API (dostupné na `/graphql`)

#### Hlavní Query operace:
```graphql
# Získání pacienta s MRI snímky
query GetPatient($id: Int!) {
  patient(id: $id) {
    id
    firstName
    lastName
    mriImages {
      id
      fileName
      dicomMetadata {
        studyDescription
        modality
      }
    }
  }
}

# Vyhledávání MRI snímků
query SearchMriImages($filter: MriImageFilterInput!) {
  searchMriImages(filter: $filter) {
    id
    fileName
    studyType
    bodyPart
    tags
  }
}

# Upload MRI snímku
mutation UploadMri($file: Upload!, $patientId: Int!) {
  uploadMriImage(file: $file, patientId: $patientId) {
    id
    fileName
    fileFormat
    dicomMetadata {
      studyDescription
      modality
    }
  }
}

# Získání binárních dat
query GetImageData($id: Int!) {
  getMriImageData(id: $id)
}
```

## Struktura projektu
```
PatientDataApp/
├── Controllers/           # REST kontrolery pro DICOM
├── Data/                 # DbContext a konfigurace
├── GraphQL/              # GraphQL implementace
│   ├── Types/           # GraphQL typy
│   ├── Query.cs         # Query definice
│   └── Mutation.cs      # Mutation definice
├── Models/               # Doménové modely
│   └── Dto/             # DTO pro REST API
├── Repositories/         # Repository pattern
├── Services/            # DICOM zpracování
└── Program.cs           # Konfigurace aplikace
```

## Vývoj

### Přidání nové migrace
```bash
dotnet ef migrations add NazevMigrace
dotnet ef database update
```

### Kompilace a spuštění
```bash
dotnet build
dotnet run
```

### Debugging
- Visual Studio: Použijte F5
- VS Code: Použijte launch.json konfiguraci
- Rider: Použijte běžnou debug konfiguraci

## Zabezpečení
- CORS je nakonfigurován pro development
- GraphQL playground je dostupný pouze v development módu
- Validace vstupních dat
- Omezení velikosti uploadovaných souborů

## Poznámky k výkonu
- Connection pooling pro PostgreSQL
- Optimalizované GraphQL dotazy
- Automatické generování náhledů
- Indexy na klíčových polích
- JSONB pro efektivní ukládání metadat

## Řešení problémů
1. Pokud se nezobrazuje GraphQL playground:
   - Zkontrolujte, že běžíte v Development módu
   - Ověřte CORS nastavení

2. Problémy s DICOM soubory:
   - Ověřte instalaci fo-dicom
   - Zkontrolujte formát souboru

3. Problémy s databází:
   - Ověřte connection string
   - Zkontrolujte práva uživatele
   - Ověřte podporu JSONB v PostgreSQL

## Licence
[Typ licence]

## Kontakt a podpora
[Kontaktní informace]