# PatientDataApp
![PatientDataApp Cover](https://utfs.io/f/NyKlEsePJFL1COQrCxdWwejYU1MKdpBnfkN9OAxasyq6XguL)

Systém pro správu pacientských dat s podporou DICOM/MRI snímků.

## Popis projektu

PatientDataApp je .NET aplikace zaměřená na správu pacientských dat včetně:
- Správy DICOM/MRI snímků
- GraphQL API pro flexibilní přístup k datům
- Diagnostických výsledků
- REST API pro práci se soubory

## Technologie

- .NET 8.0
- PostgreSQL
- GraphQL (HotChocolate)
- Docker a Docker Compose
- DICOM standard
- Cornerstone.js pro DICOM vizualizaci

## Konfigurace

Aplikace používá `.env` soubory pro konfiguraci. Pro nastavení vlastní konfigurace:

1. Zkopírujte `.env.example` do nového souboru `.env`:
   ```bash
   cp .env.example .env
   ```

2. Upravte hodnoty v `.env` souboru podle vašich potřeb:
   - Databázové připojení (DB_HOST, DB_NAME, DB_USER, DB_PASSWORD)
   - JWT autentizace (JWT_KEY, JWT_ISSUER, JWT_AUDIENCE, JWT_EXPIRY_MINUTES)
   - Nastavení serveru (ASPNETCORE_URLS, ASPNETCORE_ENVIRONMENT)
   - Limity požadavků (MAX_REQUEST_BODY_SIZE, KEEP_ALIVE_TIMEOUT_MINUTES)
   - Logování (LOG_LEVEL, ENABLE_SENSITIVE_DATA_LOGGING)
   - CORS nastavení
   - GraphQL konfigurace
   - Správa souborů a výkon

### Důležité bezpečnostní poznámky:
- Nikdy necommitujte `.env` soubor do git repozitáře
- V produkčním prostředí vždy změňte výchozí hesla a klíče
- Pro vývoj používejte jiné přístupové údaje než v produkci
- Omezte CORS nastavení pouze na potřebné domény

### Docker konfigurace:
Při použití Dockeru jsou proměnné z `.env` souboru automaticky načteny do kontejnerů:
```bash
docker-compose up -d
```

## Ukládání souborů
```json
{
  "FileStorage": {
    "MriImagesPath": "/app/data/mri-images",
    "AllowedExtensions": [".dcm", ".jpg", ".png", ".pdf"]
  }
}
```

## API Dokumentace

### REST API Endpointy

#### Získání MRI snímku
```
GET /api/file/mri/{id}
```
- Vrací soubor snímku s příslušným Content-Type
- Podporované formáty: DICOM (.dcm), JPEG (.jpg), PNG (.png), PDF (.pdf)
- Chybové stavy:
  - 404: Snímek nebo soubor nenalezen
  - 500: Interní chyba serveru

### GraphQL API

API je dostupné na `http://localhost:5001/graphql/`

### Schéma typů

#### Patient
```graphql
type Patient {
  id: ID!                           # Unikátní identifikátor
  firstName: String!                # Jméno pacienta
  lastName: String!                 # Příjmení pacienta
  dateOfBirth: DateTime!            # Datum narození
  personalId: String!               # Rodné číslo
  insuranceCompany: String          # Zdravotní pojišťovna
  lastDiagnosis: String            # Poslední diagnóza
  lastExaminationDate: DateTime    # Datum poslední prohlídky
  diagnosticResults: [DiagnosticResult!]  # Seznam diagnostických výsledků
  mriImages: [MriImage!]           # Seznam MRI snímků
  updatedAt: DateTime!             # Datum poslední aktualizace
  createdAt: DateTime!             # Datum vytvoření záznamu
}
```

#### DiagnosticResult
```graphql
type DiagnosticResult {
  id: ID!                    # Unikátní identifikátor
  patientId: Int!           # ID pacienta
  diagnosis: String!        # Diagnóza
  description: String       # Popis diagnózy
  date: DateTime!          # Datum vyšetření
  patient: Patient!        # Vazba na pacienta
}
```

#### MriImage
```graphql
type MriImage {
  id: ID!                    # Unikátní identifikátor
  patientId: Int!           # ID pacienta
  acquisitionDate: DateTime! # Datum pořízení snímku
  imagePath: String!        # Cesta k souboru snímku
  imageUrl: String!         # URL pro stažení snímku
  description: String       # Popis snímku
  findings: String         # Nálezy
  createdAt: DateTime!     # Datum vytvoření záznamu
  patient: Patient!        # Vazba na pacienta
}
```

### Frontend Integrace

#### Příklad použití s Apollo Client (React)

1. Instalace závislostí:
```bash
npm install @apollo/client graphql
# Pro DICOM vizualizaci
npm install cornerstone-core cornerstone-wado-image-loader
```

2. Nastavení Apollo Client:
```typescript
import { ApolloClient, InMemoryCache, ApolloProvider } from '@apollo/client';

const client = new ApolloClient({
  uri: 'http://localhost:5001/graphql/',
  cache: new InMemoryCache()
});

// V root komponentě
function App() {
  return (
    <ApolloProvider client={client}>
      <YourApp />
    </ApolloProvider>
  );
}
```

3. Komponenta pro zobrazení MRI snímků:

Pro běžné obrazové formáty (JPG, PNG):
```typescript
const MriViewer = ({ imageUrl }) => {
  return (
    <div>
      <img 
        src={imageUrl} 
        alt="MRI snímek"
        style={{ maxWidth: '100%', height: 'auto' }}
      />
    </div>
  );
};
```

Pro DICOM soubory:
```typescript
import * as cornerstone from 'cornerstone-core';
import * as cornerstoneWADOImageLoader from 'cornerstone-wado-image-loader';

const DicomViewer = ({ imageUrl }) => {
  const viewerRef = useRef(null);

  useEffect(() => {
    if (viewerRef.current) {
      cornerstone.enable(viewerRef.current);
      
      const loadAndDisplayImage = async () => {
        const image = await cornerstone.loadImage(imageUrl);
        cornerstone.displayImage(viewerRef.current, image);
      };

      loadAndDisplayImage();
    }

    return () => {
      if (viewerRef.current) {
        cornerstone.disable(viewerRef.current);
      }
    };
  }, [imageUrl]);

  return <div ref={viewerRef} style={{ width: '512px', height: '512px' }} />;
};
```

4. Příklad použití v komponentě:
```typescript
const MriImagesList = () => {
  const { loading, error, data } = useQuery(gql`
    query GetMriImages {
      mriImages {
        id
        imageUrl
        acquisitionDate
        description
        findings
      }
    }
  `);

  if (loading) return <p>Loading...</p>;
  if (error) return <p>Error: {error.message}</p>;

  return (
    <div>
      {data.mriImages.map(image => (
        <div key={image.id}>
          <h3>MRI snímek {image.id}</h3>
          <p>Pořízeno: {new Date(image.acquisitionDate).toLocaleDateString()}</p>
          {image.imageUrl.endsWith('.dcm') ? (
            <DicomViewer imageUrl={image.imageUrl} />
          ) : (
            <MriViewer imageUrl={image.imageUrl} />
          )}
          <p>Popis: {image.description}</p>
          <p>Nálezy: {image.findings}</p>
        </div>
      ))}
    </div>
  );
};
```

### Filtrování a řazení

API podporuje filtrování a řazení pomocí HotChocolate middleware.

#### Příklad filtrování:
```graphql
query {
  patients(
    where: {
      lastName: { contains: "Nov" }
      dateOfBirth: { gt: "1990-01-01" }
    }
  ) {
    id
    firstName
    lastName
  }
}
```

#### Příklad řazení:
```graphql
query {
  patients(
    order: [
      { lastName: ASC }
      { firstName: ASC }
    ]
  ) {
    id
    firstName
    lastName
  }
}
```

### Zpracování chyb

Backend vrací chyby ve formátu:
```json
{
  "errors": [
    {
      "message": "Error message",
      "locations": [{ "line": 2, "column": 3 }],
      "path": ["fieldName"],
      "extensions": {
        "code": "ERROR_CODE"
      }
    }
  ]
}
```

Příklad zpracování chyb na frontendu:
```typescript
const { loading, error, data } = useQuery(QUERY);

if (error) {
  if (error.graphQLErrors) {
    // Zpracování GraphQL chyb
    error.graphQLErrors.forEach(({ message, extensions }) => {
      console.error(
        `GraphQL error: ${message}`,
        `Code: ${extensions.code}`
      );
    });
  }
  if (error.networkError) {
    // Zpracování síťových chyb
    console.error(`Network error: ${error.networkError}`);
  }
}
```

### Best Practices

1. **Cachování**
   - Využívejte Apollo Cache pro optimalizaci výkonu
   - Nastavte správné cache policies pro jednotlivé typy
   - Pro MRI snímky zvažte implementaci lokálního cachování

2. **Optimalizace dotazů**
   - Požadujte pouze potřebná pole
   - Využívejte fragmenty pro znovupoužitelné části dotazů
   - Implementujte pagination pro velké seznamy

3. **Error Handling**
   - Vždy implementujte zpracování chyb
   - Poskytněte uživatelsky přívětivé chybové hlášky
   - Logujte chyby pro debugging

4. **Práce s DICOM soubory**
   - Používejte Cornerstone.js pro zobrazení DICOM souborů
   - Implementujte lazy loading pro velké DICOM soubory
   - Zvažte použití Web Workers pro zpracování DICOM dat

## Generování testovacích dat

Aplikace obsahuje vestavěný generátor testovacích dat, který vytváří realistická data českých pacientů včetně:
- Jména a příjmení
- Rodných čísel
- Pojišťoven
- Diagnóz
- Diagnostických výsledků
- MRI snímků

### Automatické generování

V development módu se testovací data automaticky vygenerují při prvním spuštění aplikace, pokud je databáze prázdná.

### Manuální generování

Testovací data můžete vygenerovat pomocí HTTP endpointu:

```bash
# Vygeneruje 50 pacientů s jejich záznamy
curl -X POST "http://localhost:8080/api/generate-test-data?patientCount=50"
```

### Přístup k testovacím datům přes GraphQL

Vygenerovaná data jsou okamžitě dostupná přes GraphQL API. Příklady dotazů:

```graphql
# Získání všech pacientů s jejich záznamy
query {
  patients {
    id
    firstName
    lastName
    dateOfBirth
    personalId
    insuranceCompany
    lastDiagnosis
    diagnosticResults {
      diagnosis
      description
      date
    }
    mriImages {
      acquisitionDate
      imageUrl
      findings
    }
  }
}

# Filtrování pacientů podle pojišťovny
query {
  patients(
    where: { insuranceCompany: { eq: "VZP" } }
  ) {
    firstName
    lastName
    insuranceCompany
    lastDiagnosis
  }
}

# Získání posledních diagnostických výsledků
query {
  diagnosticResults(
    order: { date: DESC }
    first: 10
  ) {
    diagnosis
    date
    patient {
      firstName
      lastName
    }
  }
}
```

Vygenerovaná data obsahují:
- 50 pacientů (výchozí hodnota)
- 1-5 diagnostických výsledků na pacienta
- 0-3 MRI snímků na pacienta

## Instalace a spuštění

### Požadavky
- Docker a Docker Compose
- .NET 8.0 SDK (pro lokální vývoj)
- Node.js 18+ (pro frontend)
- PostgreSQL 14+ (pro lokální vývoj bez Dockeru)

### Spuštění pomocí Dockeru (doporučeno)

1. Naklonujte repozitář:
   ```bash
   git clone https://github.com/your-org/PatientDataApp.git
   cd PatientDataApp
   ```

2. Vytvořte konfigurační soubor:
   ```bash
   cp .env.example .env
   # Upravte hodnoty v .env podle potřeby
   ```

3. Spusťte aplikaci:
   ```bash
   ./deploy.sh
   ```
   nebo manuálně:
   ```bash
   docker-compose up -d
   ```

4. Ověřte, že aplikace běží:
   ```bash
   curl http://localhost:8080/health
   ```

### Lokální vývoj

1. Nainstalujte závislosti:
   ```bash
   dotnet restore
   ```

2. Nastavte PostgreSQL:
   ```bash
   # V .env nastavte DB_HOST=localhost
   psql -U postgres -c "CREATE DATABASE patientdb;"
   ```

3. Spusťte migraci databáze:
   ```bash
   dotnet ef database update
   ```

4. Spusťte aplikaci:
   ```bash
   dotnet run
   ```

### Produkční nasazení

1. Nastavte produkční proměnné v `.env`:
   ```bash
   ASPNETCORE_ENVIRONMENT=Production
   LOG_LEVEL=Warning
   CORS_ALLOW_ANY_ORIGIN=false
   # Nastavte ostatní produkční hodnoty
   ```

2. Spusťte deploy skript:
   ```bash
   ./deploy.sh --env production
   ```

3. Ověřte nasazení:
   ```bash
   # Kontrola logů
   docker-compose logs -f api

   # Kontrola health endpointu
   curl http://localhost:8080/health
   ```

### Řešení problémů

1. **Databáze není dostupná:**
   ```bash
   # Kontrola stavu PostgreSQL
   docker-compose ps
   docker-compose logs db
   ```

2. **API není dostupné:**
   ```bash
   # Kontrola logů API
   docker-compose logs api
   ```

3. **Problémy s oprávněními:**
   ```bash
   # Nastavení oprávnění pro MRI složku
   sudo chown -R 1000:1000 ./data/mri-images
   ```

### Aktualizace aplikace

1. Stáhněte nejnovější verzi:
   ```bash
   git pull origin main
   ```

2. Aktualizujte kontejnery:
   ```bash
   docker-compose pull
   docker-compose up -d
   ```

3. Spusťte migrace:
   ```bash
   docker-compose exec api dotnet ef database update
   ```

## Databáze

Databázové schéma je automaticky inicializováno při prvním spuštění pomocí migrace. Inicializační skripty pro základní data jsou umístěny v `/init-scripts`.

## Vývoj

- Pro vývoj je dostupný GraphQL Playground na `/graphql` v development módu
- DICOM metadata lze filtrovat a zpracovávat pomocí specializovaných filtrů
- Projekt používá repository pattern pro oddělení datové vrstvy
- Soubory jsou ukládány v konfigurovaném adresáři s podporou více formátů

## Časové zóny a práce s datem a časem

Aplikace používá konzistentní přístup k práci s časovými údaji:

### Časové zóny
- Všechny časové údaje jsou interně ukládány a zpracovávány v UTC
- Databáze ukládá všechna časová data v UTC formátu
- API komunikuje výhradně v UTC formátu pomocí ISO 8601 (např. "2024-01-20T20:00:00Z")

### Implementace v různých částech systému

1. **Databázové operace**
   ```csharp
   // Správně - použití UTC času
   patient.UpdatedAt = DateTime.UtcNow;
   
   // Špatně - nepoužívat lokální čas
   patient.UpdatedAt = DateTime.Now;  // Toto nepoužívat!
   ```

2. **GraphQL API**
   ```graphql
   # Dotaz s časem v UTC
   query {
     patients(
       examinedAfter: "2024-01-20T20:00:00Z"  # UTC čas
     ) {
       id
       examinationTime  # Vráceno v UTC
     }
   }
   ```

3. **Serializace JSON**
   - Všechny DateTime hodnoty jsou automaticky serializovány do UTC
   - Používá se ISO 8601 formát s "Z" sufixem pro UTC
   - Implementováno pomocí `DateTimeConverter`

### Doporučení pro klientské aplikace
1. Vždy posílejte časové údaje v UTC formátu
2. Konvertujte UTC čas do lokální časové zóny až na úrovni prezentace
3. Pro konverzi použijte standardní knihovny vašeho programovacího jazyka

### Příklad konverze v různých jazycích

**JavaScript:**
```javascript
// Konverze UTC na lokální čas
const utcDate = new Date("2024-01-20T20:00:00Z");
const localDate = new Date(utcDate.toLocaleString());
```

**C#:**
```csharp
// Konverze mezi UTC a lokálním časem
DateTime utcTime = DateTime.UtcNow;
DateTime localTime = utcTime.ToLocalTime();

// Konverze do specifické časové zóny
TimeZoneInfo cstZone = TimeZoneInfo.FindSystemTimeZoneById("America/Chicago");
DateTime cstTime = TimeZoneInfo.ConvertTimeFromUtc(utcTime, cstZone);
```
