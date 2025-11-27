using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using pluralhealth_API.Data;
using pluralhealth_API.DTOs;
using pluralhealth_API.Models;
using Microsoft.AspNetCore.Authorization;

namespace pluralhealth_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AppointmentsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AppointmentsController> _logger;

        public AppointmentsController(ApplicationDbContext context, ILogger<AppointmentsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet("list")]
        public async Task<ActionResult<List<object>>> GetAllAppointments()
        {
            var facilityId = (int)(HttpContext.Items["FacilityId"] ?? 1);

            var appointments = await _context.Appointments
                .Include(a => a.Patient)
                .Include(a => a.Clinic)
                .Include(a => a.AppointmentType)
                .Where(a => a.FacilityId == facilityId)
                .OrderBy(a => a.StartTime) // Default ascending by date/time
                .Select(a => new 
                {
                    id = a.Id,
                    patientName = a.Patient!.Name,
                    patientCode = a.Patient.Code,
                    clinicName = a.Clinic!.Name,
                    appointmentTypeName = a.AppointmentType!.Name,
                    startTime = a.StartTime,
                    durationMinutes = a.DurationMinutes,
                    status = a.Status
                })
                .ToListAsync();

            return Ok(appointments);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Appointment>> GetAppointment(int id)
        {
            var facilityId = (int)(HttpContext.Items["FacilityId"] ?? 1);

            var appointment = await _context.Appointments
                .Include(a => a.Patient)
                .Include(a => a.Clinic)
                .Include(a => a.AppointmentType)
                .FirstOrDefaultAsync(a => a.Id == id && a.FacilityId == facilityId);

            if (appointment == null)
                return NotFound();

            return Ok(appointment);
        }

        [HttpGet("patient/{patientId}")]
        public async Task<ActionResult<List<Appointment>>> GetPatientAppointments(int patientId)
        {
            var facilityId = (int)(HttpContext.Items["FacilityId"] ?? 1);

            var appointments = await _context.Appointments
                .Include(a => a.Clinic)
                .Include(a => a.AppointmentType)
                .Where(a => a.PatientId == patientId && a.FacilityId == facilityId)
                .OrderBy(a => a.StartTime)
                .ToListAsync();

            return Ok(appointments);
        }

        [HttpPost]
        public async Task<ActionResult<Appointment>> CreateAppointment([FromBody] CreateAppointmentRequest request)
        {
            var facilityId = (int)(HttpContext.Items["FacilityId"] ?? 1);
            var userId = (int)(HttpContext.Items["UserId"] ?? 1);

            // Validation with user-friendly messages
            if (request.PatientId <= 0)
                return BadRequest("Please select a patient. Search for a patient first and select them from the dropdown.");
            
            if (request.ClinicId <= 0)
                return BadRequest("Please select a clinic.");
            
            if (request.AppointmentTypeId <= 0)
                return BadRequest("Please select an appointment type.");

            // Allow appointments starting from now (with 1 minute buffer, using local time)
            if (request.StartTime < DateTime.Now.AddMinutes(-1))
                return BadRequest($"Appointment time cannot be in the past. Requested: {request.StartTime:yyyy-MM-dd HH:mm:ss}, Current: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");

            // Verify patient exists and belongs to facility
            var patient = await _context.Patients
                .FirstOrDefaultAsync(p => p.Id == request.PatientId && p.FacilityId == facilityId);
            if (patient == null)
                return BadRequest("The selected patient was not found. Please search and select a valid patient.");

            // Verify clinic exists and belongs to facility
            var clinic = await _context.Clinics
                .FirstOrDefaultAsync(c => c.Id == request.ClinicId && c.FacilityId == facilityId);
            if (clinic == null)
                return BadRequest("The selected clinic was not found. Please select a valid clinic.");

            // Get appointment type and default duration
            var appointmentType = await _context.AppointmentTypes
                .FirstOrDefaultAsync(at => at.Id == request.AppointmentTypeId && at.FacilityId == facilityId);
            if (appointmentType == null)
                return BadRequest("The selected appointment type was not found. Please select a valid appointment type.");

            var duration = request.DurationMinutes ?? appointmentType.DefaultDurationMinutes;

            var appointment = new Appointment
            {
                PatientId = request.PatientId,
                ClinicId = request.ClinicId,
                AppointmentTypeId = request.AppointmentTypeId,
                StartTime = request.StartTime,
                DurationMinutes = duration,
                Status = "Scheduled",
                FacilityId = facilityId,
                CreatedBy = userId,
                CreatedAt = DateTime.Now
            };

            _context.Appointments.Add(appointment);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Appointment created. AppointmentId: {AppointmentId}, PatientId: {PatientId}, FacilityId: {FacilityId}, CreatedBy: {CreatedBy}",
                appointment.Id, appointment.PatientId, facilityId, userId);

            // Return DTO to avoid circular reference issues
            var response = new AppointmentResponse
            {
                Id = appointment.Id,
                PatientId = appointment.PatientId,
                ClinicId = appointment.ClinicId,
                AppointmentTypeId = appointment.AppointmentTypeId,
                StartTime = appointment.StartTime,
                DurationMinutes = appointment.DurationMinutes,
                Status = appointment.Status,
                FacilityId = appointment.FacilityId,
                CreatedBy = appointment.CreatedBy,
                CreatedAt = appointment.CreatedAt
            };

            return CreatedAtAction(nameof(GetAppointment), new { id = appointment.Id }, response);
        }
    }
}

