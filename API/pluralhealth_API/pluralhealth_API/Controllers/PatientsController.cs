using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using pluralhealth_API.Data;
using pluralhealth_API.DTOs;
using pluralhealth_API.Models;

namespace pluralhealth_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PatientsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<PatientsController> _logger;

        public PatientsController(ApplicationDbContext context, ILogger<PatientsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<object>> GetPatient(int id)
        {
            var facilityId = (int)(HttpContext.Items["FacilityId"] ?? 1);

            var patient = await _context.Patients
                .Where(p => p.Id == id && p.FacilityId == facilityId)
                .Select(p => new { p.Id, p.Code, p.Name, p.Phone, p.WalletBalance, p.Currency })
                .FirstOrDefaultAsync();

            if (patient == null)
                return NotFound();

            return Ok(patient);
        }

        [HttpGet("search")]
        public async Task<ActionResult<List<object>>> SearchPatients([FromQuery] string q)
        {
            var facilityId = (int)(HttpContext.Items["FacilityId"] ?? 1);

            if (string.IsNullOrWhiteSpace(q))
                return Ok(new List<object>());

            var patients = await _context.Patients
                .Where(p => p.FacilityId == facilityId &&
                    (p.Name.Contains(q) || p.Code.Contains(q) || (p.Phone != null && p.Phone.Contains(q))))
                .Select(p => new { p.Id, p.Code, p.Name, p.Phone, p.WalletBalance, p.Currency })
                .Take(20)
                .ToListAsync();

            return Ok(patients);
        }

        [HttpPost]
        public async Task<ActionResult<object>> CreatePatient([FromBody] CreatePatientRequest request)
        {
            var facilityId = (int)(HttpContext.Items["FacilityId"] ?? 1);

            // Validation
            if (string.IsNullOrWhiteSpace(request.Name))
                return BadRequest("Patient name is required.");

            // Generate patient code
            var lastPatient = await _context.Patients
                .Where(p => p.FacilityId == facilityId)
                .OrderByDescending(p => p.Id)
                .FirstOrDefaultAsync();

            int nextNumber = 1;
            if (lastPatient != null && !string.IsNullOrEmpty(lastPatient.Code))
            {
                // Extract number from code (e.g., "P001" -> 1)
                var codeNumber = lastPatient.Code.Replace("P", "");
                if (int.TryParse(codeNumber, out var num))
                {
                    nextNumber = num + 1;
                }
            }

            var patientCode = $"P{nextNumber:D3}";

            // Check if code already exists (shouldn't happen, but safety check)
            var existingPatient = await _context.Patients
                .FirstOrDefaultAsync(p => p.FacilityId == facilityId && p.Code == patientCode);

            if (existingPatient != null)
            {
                // If code exists, find next available
                var allCodes = await _context.Patients
                    .Where(p => p.FacilityId == facilityId)
                    .Select(p => p.Code)
                    .ToListAsync();

                nextNumber = 1;
                while (allCodes.Contains($"P{nextNumber:D3}"))
                {
                    nextNumber++;
                }
                patientCode = $"P{nextNumber:D3}";
            }

            var patient = new Patient
            {
                Code = patientCode,
                Name = request.Name.Trim(),
                Phone = request.Phone?.Trim(),
                WalletBalance = request.WalletBalance,
                Currency = request.Currency ?? "NGN",
                FacilityId = facilityId,
                Status = "Processing"
            };

            _context.Patients.Add(patient);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Patient created. PatientId: {PatientId}, Code: {Code}, Name: {Name}, FacilityId: {FacilityId}",
                patient.Id, patient.Code, patient.Name, facilityId);

            return CreatedAtAction(nameof(GetPatient), new { id = patient.Id }, new
            {
                patient.Id,
                patient.Code,
                patient.Name,
                patient.Phone,
                patient.WalletBalance,
                patient.Currency
            });
        }
    }
}

