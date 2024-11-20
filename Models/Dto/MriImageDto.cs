namespace PatientDataApp.Models
{
    public class MriImageDto
    {
        public int Id { get; set; }
        public string FileName { get; set; }
        public string FileFormat { get; set; }
        public DateTime UploadedAt { get; set; }
    }

    public class MriImageUploadDto
    {
        public int PatientId { get; set; }
        public IFormFile File { get; set; }
    }
} 