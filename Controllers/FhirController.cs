using Microsoft.AspNetCore.Mvc;
using PatientDataApp.Data;
using PatientDataApp.Models;
using Hl7.Fhir.Model;
using Hl7.Fhir.Serialization;

namespace PatientDataApp.Controllers
{
    [Route("api/fhir")]
    [ApiController]
    public class FhirController : ControllerBase
    {
        private readonly PatientDbContext _context;

        public FhirController(PatientDbContext context)
        {
            _context = context;
        }

        [HttpGet("Patient/{id}")]
        public IActionResult GetPatientFhir(int id)
        {
            var patientEntity = _context.Patients.FirstOrDefault(p => p.Id == id);
            if (patientEntity == null)
            {
                return NotFound();
            }

            var fhirPatient = new Hl7.Fhir.Model.Patient
            {
                Id = patientEntity.Id.ToString(),
                Name = new List<HumanName> 
                { 
                    new HumanName 
                    { 
                        Given = new[] { patientEntity.FirstName },
                        Family = patientEntity.LastName
                    } 
                },
                BirthDate = patientEntity.DateOfBirth.ToString("yyyy-MM-dd"),
                Identifier = new List<Identifier>
                {
                    new Identifier
                    {
                        System = "http://example.org/personal-id",
                        Value = patientEntity.PersonalId
                    }
                }
            };

            if (!string.IsNullOrEmpty(patientEntity.LastDiagnosis))
            {
                fhirPatient.Extension.Add(new Extension
                {
                    Url = "http://example.org/last-diagnosis",
                    Value = new FhirString(patientEntity.LastDiagnosis)
                });
            }

            return Ok(new FhirJsonSerializer().SerializeToString(fhirPatient));
        }
    }
}
