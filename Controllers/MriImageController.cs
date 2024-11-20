using Microsoft.AspNetCore.Mvc;
using PatientDataApp.Models;
using PatientDataApp.Repositories;
using System.IO;
using PatientDataApp.Services;

namespace PatientDataApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MriImageController : ControllerBase
    {
        private readonly IMriImageRepository _repository;

        public MriImageController(IMriImageRepository repository)
        {
            _repository = repository;
        }

        [HttpGet("patient/{patientId}")]
        public async Task<ActionResult<IEnumerable<MriImageDto>>> GetByPatientId(int patientId)
        {
            var images = await _repository.GetAllByPatientIdAsync(patientId);
            var dtos = images.Select(i => new MriImageDto
            {
                Id = i.Id,
                FileName = i.FileName,
                FileFormat = i.FileFormat,
                UploadedAt = i.UploadedAt
            });
            return Ok(dtos);
        }

        [HttpGet("{id}/download")]
        public async Task<IActionResult> Download(int id)
        {
            var image = await _repository.GetByIdAsync(id);
            if (image == null)
                return NotFound();

            return File(image.FileData, GetContentType(image.FileFormat), image.FileName);
        }

        [HttpPost("upload")]
        public async Task<IActionResult> Upload([FromForm] MriImageUploadDto uploadDto)
        {
            if (uploadDto.File == null || uploadDto.File.Length == 0)
                return BadRequest("Žádný soubor nebyl nahrán.");

            using var memoryStream = new MemoryStream();
            await uploadDto.File.CopyToAsync(memoryStream);

            var mriImage = new MriImage
            {
                PatientId = uploadDto.PatientId,
                FileName = uploadDto.File.FileName,
                FileFormat = Path.GetExtension(uploadDto.File.FileName).TrimStart('.'),
                FileData = memoryStream.ToArray(),
            };

            await _repository.AddAsync(mriImage);
            return Ok(new { id = mriImage.Id, message = "Snímek byl úspěšně nahrán" });
        }

        [HttpPost("upload/dicom")]
        public async Task<IActionResult> UploadDicom([FromForm] MriImageUploadDto uploadDto, 
            [FromServices] DicomService dicomService)
        {
            if (uploadDto.File == null || uploadDto.File.Length == 0)
                return BadRequest("Žádný soubor nebyl nahrán.");

            using var memoryStream = new MemoryStream();
            await uploadDto.File.CopyToAsync(memoryStream);
            var dicomData = memoryStream.ToArray();

            try
            {
                // Extrahujeme metadata z DICOM souboru
                var metadata = await dicomService.ExtractDicomMetadata(dicomData);
                
                // Převedeme DICOM na JPEG pro náhled
                var jpegData = await dicomService.ConvertDicomToJpeg(dicomData);

                var mriImage = new MriImage
                {
                    PatientId = uploadDto.PatientId,
                    FileName = uploadDto.File.FileName,
                    FileFormat = "dcm",
                    FileData = dicomData,
                    DicomMetadata = metadata
                };

                // Uložíme také JPEG verzi pro náhled
                var jpegPreview = new MriImage
                {
                    PatientId = uploadDto.PatientId,
                    FileName = Path.ChangeExtension(uploadDto.File.FileName, "jpg"),
                    FileFormat = "jpg",
                    FileData = jpegData,
                    IsPreview = true,
                    OriginalImageId = mriImage.Id
                };

                await _repository.AddAsync(mriImage);
                await _repository.AddAsync(jpegPreview);

                return Ok(new { 
                    id = mriImage.Id, 
                    previewId = jpegPreview.Id,
                    metadata = metadata,
                    message = "DICOM snímek byl úspěšně nahrán a zpracován" 
                });
            }
            catch (Exception ex)
            {
                return BadRequest($"Chyba při zpracování DICOM souboru: {ex.Message}");
            }
        }

        private string GetContentType(string fileFormat)
        {
            return fileFormat.ToLower() switch
            {
                "jpg" or "jpeg" => "image/jpeg",
                "png" => "image/png",
                "dcm" => "application/dicom",
                _ => "application/octet-stream"
            };
        }
    }
} 