using System.ComponentModel.DataAnnotations;

namespace PatientDataApp.Models;

public class Patient
{
    public int Id { get; set; }
    
    [Required]
    [MaxLength(100)]
    public required string FirstName { get; set; }
    
    [Required]
    [MaxLength(100)]
    public required string LastName { get; set; }
    
    [Required]
    public DateTime DateOfBirth { get; set; }

    [Required]
    [MaxLength(20)]
    public required string PersonalId { get; set; }
    
    [Required]
    [MaxLength(1)]
    public required string Gender { get; set; }
    
    [MaxLength(200)]
    public string? Address { get; set; }
    
    [Phone]
    [MaxLength(20)]
    public string? PhoneNumber { get; set; }
    
    [EmailAddress]
    [MaxLength(100)]
    public string? Email { get; set; }
    
    public string? LastDiagnosis { get; set; }
    
    [MaxLength(50)]
    public string? InsuranceCompany { get; set; }
    
    public DateTime? LastExaminationDate { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    // Vztahy
    public virtual ICollection<DiagnosticResult>? DiagnosticResults { get; set; }
    public virtual ICollection<MriImage>? MriImages { get; set; }
    
    // Pomocné metody
    public int GetAge()
    {
        var today = DateTime.Today;
        var age = today.Year - DateOfBirth.Year;
        if (DateOfBirth.Date > today.AddYears(-age)) age--;
        return age;
    }
    
    public string GetFullName()
    {
        return $"{FirstName} {LastName}";
    }
}
