using System.ComponentModel.DataAnnotations;

namespace PatientDataApp.Models;

public class MriImage
{
    public int Id { get; set; }
    
    [Required]
    public int PatientId { get; set; }
    
    [Required]
    public DateTime AcquisitionDate { get; set; }
    
    [Required]
    [MaxLength(255)]
    public required string ImagePath { get; set; }
    
    [MaxLength(500)]
    public string? Description { get; set; }
    
    public string? Findings { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    // Vztah
    public virtual Patient? Patient { get; set; }
}
