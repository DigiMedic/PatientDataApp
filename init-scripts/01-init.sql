-- Vytvoření tabulky pro pacienty
CREATE TABLE IF NOT EXISTS "Patients" (
    "Id" SERIAL PRIMARY KEY,
    "FirstName" VARCHAR(100) NOT NULL,
    "LastName" VARCHAR(100) NOT NULL,
    "DateOfBirth" TIMESTAMP NOT NULL,
    "PersonalId" VARCHAR(20) NOT NULL UNIQUE,
    "LastDiagnosis" TEXT,
    "InsuranceCompany" VARCHAR(50),
    "LastExaminationDate" TIMESTAMP,
    "CreatedAt" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "UpdatedAt" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
);

-- Vytvoření tabulky pro diagnostické výsledky
CREATE TABLE IF NOT EXISTS "DiagnosticResults" (
    "Id" SERIAL PRIMARY KEY,
    "PatientId" INTEGER NOT NULL,
    "Diagnosis" TEXT NOT NULL,
    "Description" TEXT,
    "Date" TIMESTAMP NOT NULL,
    "CreatedAt" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT "FK_DiagnosticResults_Patients" FOREIGN KEY ("PatientId") REFERENCES "Patients"("Id") ON DELETE CASCADE
);

-- Vytvoření tabulky pro MRI snímky
CREATE TABLE IF NOT EXISTS "MriImages" (
    "Id" SERIAL PRIMARY KEY,
    "PatientId" INTEGER NOT NULL,
    "ImagePath" VARCHAR(255) NOT NULL,
    "Description" VARCHAR(500),
    "Findings" TEXT,
    "AcquisitionDate" TIMESTAMP NOT NULL,
    "CreatedAt" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT "FK_MriImages_Patients" FOREIGN KEY ("PatientId") REFERENCES "Patients"("Id") ON DELETE CASCADE
);

-- Vytvoření indexů pro Patients
CREATE INDEX IF NOT EXISTS "IX_Patients_LastName_FirstName" ON "Patients"("LastName", "FirstName");
CREATE UNIQUE INDEX IF NOT EXISTS "IX_Patients_PersonalId" ON "Patients"("PersonalId");
CREATE INDEX IF NOT EXISTS "IX_Patients_LastExaminationDate" ON "Patients"("LastExaminationDate");

-- Vytvoření indexů pro DiagnosticResults
CREATE INDEX IF NOT EXISTS "IX_DiagnosticResults_PatientId" ON "DiagnosticResults"("PatientId");
CREATE INDEX IF NOT EXISTS "IX_DiagnosticResults_Date" ON "DiagnosticResults"("Date");

-- Vytvoření indexů pro MriImages
CREATE INDEX IF NOT EXISTS "IX_MriImages_PatientId" ON "MriImages"("PatientId");
CREATE INDEX IF NOT EXISTS "IX_MriImages_AcquisitionDate" ON "MriImages"("AcquisitionDate");

-- Trigger pro automatickou aktualizaci UpdatedAt
CREATE OR REPLACE FUNCTION update_updated_at_column()
RETURNS TRIGGER AS $$
BEGIN
    NEW."UpdatedAt" = CURRENT_TIMESTAMP;
    RETURN NEW;
END;
$$ language 'plpgsql';

CREATE TRIGGER update_patients_updated_at
    BEFORE UPDATE ON "Patients"
    FOR EACH ROW
    EXECUTE FUNCTION update_updated_at_column();
