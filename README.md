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

### Ukládání souborů
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

## Nasazení

### Automatické nasazení (doporučeno)

1. Naklonujte repozitář:
```bash
git clone [URL_repozitáře]
cd PatientDataApp
```

2. Spusťte deployment script:
```bash
chmod +x deploy.sh  # Nastavení práv pro spuštění
./deploy.sh
```

Script automaticky:
- Zastaví běžící kontejnery
- Vyčistí Docker images
- Nastaví správný port (5001)
- Sestaví a spustí aplikaci
- Zobrazí průběh nasazení a logy

Po dokončení bude aplikace dostupná na:
- GraphQL API: http://localhost:5001/graphql/
- REST API pro soubory: http://localhost:5001/api/file/

### Manuální nasazení

Pouze pokud nemůžete použít automatické nasazení:

1. Požadavky:
   - .NET 8.0 SDK
   - PostgreSQL
   - Nastavený connection string v appsettings.json
   - Nastavená cesta pro ukládání souborů v appsettings.json

2. Spuštění:
```bash
dotnet restore
dotnet build
dotnet run
```

## Databáze

Databázové schéma je automaticky inicializováno při prvním spuštění pomocí migrace. Inicializační skripty pro základní data jsou umístěny v `/init-scripts`.

## Vývoj

- Pro vývoj je dostupný GraphQL Playground na `/graphql` v development módu
- DICOM metadata lze filtrovat a zpracovávat pomocí specializovaných filtrů
- Projekt používá repository pattern pro oddělení datové vrstvy
- Soubory jsou ukládány v konfigurovaném adresáři s podporou více formátů
