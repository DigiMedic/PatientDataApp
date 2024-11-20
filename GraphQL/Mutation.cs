using HotChocolate;
using HotChocolate.Types;
using PatientDataApp.Data;
using PatientDataApp.Models;
using PatientDataApp.GraphQL;
using Microsoft.EntityFrameworkCore;
using PatientDataApp.Repositories;
using System.IO;

namespace PatientDataApp.GraphQL;

[GraphQLDescription("Mutations for managing patients")]
public class Mutation
{
    private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;

    public Mutation(IDbContextFactory<ApplicationDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    [GraphQLDescription("Add a new patient")]
    public async Task<Patient> AddPatient(
        string name, 
        int age, 
        string lastDiagnosis)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        try 
        {
            var patient = new Patient
            {
                Name = name,
                Age = age,
                LastDiagnosis = lastDiagnosis
            };

            await context.Patients.AddAsync(patient);
            await context.SaveChangesAsync();

            return patient;
        }
        catch (Exception ex)
        {
            throw new GraphQLException(
                new Error("Failed to add patient", ex.Message));
        }
    }

    [GraphQLDescription("Update existing patient")]
    public async Task<Patient> UpdatePatient(int id, string name, int age, string lastDiagnosis)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        var patient = await context.Patients.FirstOrDefaultAsync(p => p.Id == id);

        if (patient == null)
        {
            throw new GraphQLException(new Error("Patient not found", "NOT_FOUND"));
        }

        patient.Name = name;
        patient.Age = age;
        patient.LastDiagnosis = lastDiagnosis;

        await context.SaveChangesAsync();

        return patient;
    }

    [GraphQLDescription("Smaže MRI snímek podle ID")]
    public async Task<bool> DeleteMriImage(
        [Service] IMriImageRepository repository,
        int id)
    {
        await repository.DeleteAsync(id);
        return true;
    }

    [GraphQLDescription("Aktualizuje metadata MRI snímku")]
    public async Task<MriImage> UpdateMriImageMetadata(
        [Service] IMriImageRepository repository,
        int id,
        string fileName)
    {
        var image = await repository.GetByIdAsync(id);
        if (image == null)
            throw new GraphQLException("MRI snímek nebyl nalezen");

        image.FileName = fileName;
        await repository.UpdateAsync(image);
        return image;
    }

    [GraphQLDescription("Přidá tagy k MRI snímku")]
    public async Task<MriImage> AddMriImageTags(
        [Service] IMriImageRepository repository,
        int id,
        Dictionary<string, string> tags)
    {
        var image = await repository.GetByIdAsync(id);
        if (image == null)
            throw new GraphQLException("MRI snímek nebyl nalezen");

        foreach (var tag in tags)
        {
            image.Tags[tag.Key] = tag.Value;
        }

        await repository.UpdateAsync(image);
        return image;
    }

    [GraphQLDescription("Aktualizuje metadata DICOM snímku")]
    public async Task<MriImage> UpdateDicomMetadata(
        [Service] IMriImageRepository repository,
        int id,
        DicomMetadataInput metadata)
    {
        var image = await repository.GetByIdAsync(id);
        if (image == null)
            throw new GraphQLException("MRI snímek nebyl nalezen");

        image.DicomMetadata = new DicomMetadata
        {
            StudyDescription = metadata.StudyDescription,
            SeriesDescription = metadata.SeriesDescription,
            BodyPartExamined = metadata.BodyPartExamined,
            // ... další metadata
        };

        await repository.UpdateAsync(image);
        return image;
    }

    [GraphQLDescription("Upload MRI snímku")]
    public async Task<MriImage> UploadMriImage(
        [Service] IMriImageRepository repository,
        [Service] DicomService dicomService,
        IFile file,
        int patientId)
    {
        using var stream = file.OpenReadStream();
        using var memoryStream = new MemoryStream();
        await stream.CopyToAsync(memoryStream);
        var fileData = memoryStream.ToArray();

        var fileName = file.Name;
        var fileFormat = Path.GetExtension(fileName).TrimStart('.');

        var mriImage = new MriImage
        {
            PatientId = patientId,
            FileName = fileName,
            FileFormat = fileFormat,
            FileData = fileData
        };

        if (fileFormat.ToLower() == "dcm")
        {
            var metadata = await dicomService.ExtractDicomMetadata(fileData);
            var jpegData = await dicomService.ConvertDicomToJpeg(fileData);
            
            mriImage.DicomMetadata = metadata;

            // Vytvoření náhledu
            var preview = new MriImage
            {
                PatientId = patientId,
                FileName = Path.ChangeExtension(fileName, "jpg"),
                FileFormat = "jpg",
                FileData = jpegData,
                IsPreview = true
            };

            await repository.AddAsync(preview);
        }

        await repository.AddAsync(mriImage);
        return mriImage;
    }

    [GraphQLDescription("Získání binárních dat snímku")]
    public async Task<byte[]> GetMriImageData(
        [Service] IMriImageRepository repository,
        int id)
    {
        var image = await repository.GetByIdAsync(id);
        if (image == null)
            throw new GraphQLException("MRI snímek nebyl nalezen");

        return image.FileData;
    }
}
