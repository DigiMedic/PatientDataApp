using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace PatientDataApp.Models
{
    public class MriImage
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int PatientId { get; set; }

        [Required]
        [MaxLength(255)]
        public string FileName { get; set; }

        [MaxLength(50)]
        public string FileFormat { get; set; }

        [Required]
        public byte[] FileData { get; set; }

        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

        public bool IsPreview { get; set; }

        public int? OriginalImageId { get; set; }

        [JsonIgnore]
        [ForeignKey("PatientId")]
        public virtual Patient Patient { get; set; }

        public DicomMetadata DicomMetadata { get; set; }

        public string Description { get; set; }

        public string StudyType { get; set; }

        public string BodyPart { get; set; }

        public Dictionary<string, string> Tags { get; set; } = new();
    }

    public class DicomMetadata
    {
        public string PatientName { get; set; }

        public DateTime? StudyDate { get; set; }

        public string Modality { get; set; }

        public string StudyDescription { get; set; }

        public string SeriesDescription { get; set; }

        public string BodyPartExamined { get; set; }

        public string StudyInstanceUid { get; set; }

        public string SeriesInstanceUid { get; set; }
    }
} 