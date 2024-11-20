namespace PatientDataApp.Models.Dto
{
    public class MriImageFilterInput
    {
        public int? PatientId { get; set; }
        public string StudyType { get; set; }
        public string BodyPart { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public List<string> Tags { get; set; }
    }
} 