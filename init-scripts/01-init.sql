CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

-- Vytvoření tabulky patients
CREATE TABLE IF NOT EXISTS patients (
    id SERIAL PRIMARY KEY,
    name VARCHAR(255) NOT NULL,
    family_name VARCHAR(255) NOT NULL,
    given_name VARCHAR(255) NOT NULL,
    gender VARCHAR(50),
    birth_date DATE,
    address VARCHAR(255),
    phone VARCHAR(50),
    email VARCHAR(255)
);

-- Vytvoření tabulky conditions
CREATE TABLE IF NOT EXISTS conditions (
    id SERIAL PRIMARY KEY,
    patient_id INT NOT NULL,
    code VARCHAR(255) NOT NULL,
    display_name VARCHAR(255) NOT NULL,
    severity VARCHAR(50),
    status VARCHAR(50),
    date DATE,
    FOREIGN KEY (patient_id) REFERENCES patients(id)
);

-- Vytvoření tabulky medications
CREATE TABLE IF NOT EXISTS medications (
    id SERIAL PRIMARY KEY,
    patient_id INT NOT NULL,
    code VARCHAR(255) NOT NULL,
    name VARCHAR(255) NOT NULL,
    dosage VARCHAR(255),
    route VARCHAR(50),
    start_date DATE,
    FOREIGN KEY (patient_id) REFERENCES patients(id)
);

-- Vytvoření tabulky mri_images
CREATE TABLE IF NOT EXISTS mri_images (
    id SERIAL PRIMARY KEY,
    patient_id INT NOT NULL,
    file_name VARCHAR(255) NOT NULL,
    file_format VARCHAR(50),
    file_data BYTEA NOT NULL,
    uploaded_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (patient_id) REFERENCES patients(id)
);

-- Vytvoření tabulky observations
CREATE TABLE IF NOT EXISTS observations (
    id SERIAL PRIMARY KEY,
    patient_id INT NOT NULL,
    code VARCHAR(255) NOT NULL,
    display_name VARCHAR(255) NOT NULL,
    value NUMERIC,
    unit VARCHAR(50),
    observation_date DATE,
    FOREIGN KEY (patient_id) REFERENCES patients(id)
);

-- Vytvoření tabulky users
CREATE TABLE IF NOT EXISTS users (
    id SERIAL PRIMARY KEY,
    username VARCHAR(50) UNIQUE NOT NULL,
    password_hash VARCHAR(255) NOT NULL,
    role VARCHAR(50) NOT NULL
);

-- Vytvoření indexů
CREATE INDEX IF NOT EXISTS idx_patients_name ON patients (name);
CREATE INDEX IF NOT EXISTS idx_conditions_code ON conditions (code);
CREATE INDEX IF NOT EXISTS idx_medications_code ON medications (code);
CREATE INDEX IF NOT EXISTS idx_mri_images_patient_id ON mri_images (patient_id);
CREATE INDEX IF NOT EXISTS idx_observations_patient_id ON observations (patient_id); 