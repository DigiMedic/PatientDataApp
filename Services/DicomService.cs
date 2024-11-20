using FellowOakDicom;
using PatientDataApp.Models;

namespace PatientDataApp.Services
{
    public class DicomService
    {
        public async Task<byte[]> ConvertDicomToJpeg(byte[] dicomData)
        {
            var dicomFile = await DicomFile.OpenAsync(new MemoryStream(dicomData));
            var dicomImage = new DicomImage(dicomFile.Dataset);
            
            using var memoryStream = new MemoryStream();
            await dicomImage.RenderImage().AsJpeg().SaveAsync(memoryStream);
            return memoryStream.ToArray();
        }

        public async Task<DicomMetadata> ExtractDicomMetadata(byte[] dicomData)
        {
            var dicomFile = await DicomFile.OpenAsync(new MemoryStream(dicomData));
            var dataset = dicomFile.Dataset;

            return new DicomMetadata
            {
                PatientName = dataset.GetString(DicomTag.PatientName),
                StudyDate = dataset.GetDateTime(DicomTag.StudyDate),
                Modality = dataset.GetString(DicomTag.Modality),
                StudyDescription = dataset.GetString(DicomTag.StudyDescription)
            };
        }
    }

    public class DicomMetadata
    {
        public string PatientName { get; set; }
        public DateTime? StudyDate { get; set; }
        public string Modality { get; set; }
        public string StudyDescription { get; set; }
    }
} 