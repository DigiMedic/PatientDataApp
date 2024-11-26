using System.ComponentModel.DataAnnotations;

namespace PatientDataApp.Models;

public class DiagnosticResult
{
    public int Id { get; set; }
    public int PatientId { get; set; }
    [Required]
    public string Diagnosis { get; set; }
    public string? Description { get; set; }
    public DateTime Date { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    public virtual Patient? Patient { get; set; }
}
