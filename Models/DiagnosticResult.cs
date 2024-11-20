namespace PatientDataApp.Models;

public class DiagnosticResult
{
    public int Id { get; set; }
    public int PatientId { get; set; }
    public required string Diagnosis { get; set; }
    public string? Description { get; set; }
    public DateTime Date { get; set; }
    
    public virtual Patient? Patient { get; set; }
}
