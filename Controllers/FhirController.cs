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
        private readonly ApplicationDbContext _context;

        public FhirController(ApplicationDbContext context)
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
                Name = new List<HumanName> { new HumanName { Given = new[] { patientEntity.Name } } },
                BirthDate = (DateTime.Now.Year - patientEntity.Age).ToString()
            };

            return Ok(new FhirJsonSerializer().SerializeToString(fhirPatient));
        }
    }
}
