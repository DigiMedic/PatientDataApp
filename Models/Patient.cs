namespace PatientDataApp.Models;

public class Patient
{
    public int Id { get; set; }
    public string Name { get; set; }
    public int Age { get; set; }
    public required string LastDiagnosis { get; set; }
}