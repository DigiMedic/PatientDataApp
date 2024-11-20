# PatientDataApp

Systém pro správu pacientských dat s podporou DICOM/MRI snímků.

## Popis projektu

PatientDataApp je .NET aplikace zaměřená na správu pacientských dat včetně:
- Správy DICOM/MRI snímků
- GraphQL API pro flexibilní přístup k datům
- Diagnostických výsledků

## Technologie

- .NET 8.0
- PostgreSQL
- GraphQL (HotChocolate)
- Docker a Docker Compose
- DICOM standard

## GraphQL API Dokumentace

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

3. Příklad dotazu na pacienty:
```typescript
import { useQuery, gql } from '@apollo/client';

const GET_PATIENTS = gql`
  query GetPatients {
    patients {
      id
      firstName
      lastName
      dateOfBirth
      lastDiagnosis
      diagnosticResults {
        diagnosis
        date
      }
    }
  }
`;

function PatientsList() {
  const { loading, error, data } = useQuery(GET_PATIENTS);

  if (loading) return <p>Loading...</p>;
  if (error) return <p>Error: {error.message}</p>;

  return (
    <ul>
      {data.patients.map(patient => (
        <li key={patient.id}>
          {patient.firstName} {patient.lastName}
        </li>
      ))}
    </ul>
  );
}
```

4. Příklad mutace pro vytvoření pacienta:
```typescript
import { useMutation, gql } from '@apollo/client';

const CREATE_PATIENT = gql`
  mutation CreatePatient(
    $firstName: String!
    $lastName: String!
    $dateOfBirth: DateTime!
    $personalId: String!
  ) {
    createPatient(
      firstName: $firstName
      lastName: $lastName
      dateOfBirth: $dateOfBirth
      personalId: $personalId
    ) {
      id
      firstName
      lastName
    }
  }
`;

function CreatePatientForm() {
  const [createPatient, { data, loading, error }] = useMutation(CREATE_PATIENT);

  const handleSubmit = (e) => {
    e.preventDefault();
    createPatient({
      variables: {
        firstName: "Jan",
        lastName: "Novák",
        dateOfBirth: "1990-01-01T00:00:00",
        personalId: "9001011234"
      }
    });
  };

  // Render form...
}
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

2. **Optimalizace dotazů**
   - Požadujte pouze potřebná pole
   - Využívejte fragmenty pro znovupoužitelné části dotazů
   - Implementujte pagination pro velké seznamy

3. **Error Handling**
   - Vždy implementujte zpracování chyb
   - Poskytněte uživatelsky přívětivé chybové hlášky
   - Logujte chyby pro debugging

4. **Typová bezpečnost**
   - Využívejte GraphQL Code Generator pro generování TypeScript typů
   - Implementujte strict type checking

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

### Manuální nasazení

Pouze pokud nemůžete použít automatické nasazení:

1. Požadavky:
   - .NET 8.0 SDK
   - PostgreSQL
   - Nastavený connection string v appsettings.json

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
