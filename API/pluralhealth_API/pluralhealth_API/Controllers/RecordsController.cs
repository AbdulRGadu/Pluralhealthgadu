using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using pluralhealth_API.Data;
using pluralhealth_API.DTOs;
using pluralhealth_API.Middleware;

namespace pluralhealth_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RecordsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<RecordsController> _logger;

        public RecordsController(ApplicationDbContext context, ILogger<RecordsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<PagedResult<RecordsResponse>>> GetRecords(
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            [FromQuery] int? clinicId,
            [FromQuery] string? search,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] string sortBy = "time")
        {
            var facilityId = (int)(HttpContext.Items["FacilityId"] ?? 1);

            // Default to today only if no date range provided (using local time)
            if (!startDate.HasValue)
                startDate = DateTime.Now.Date;
            if (!endDate.HasValue)
                endDate = startDate.Value.AddDays(1).AddTicks(-1); // End of today

            _logger.LogInformation("Records list loaded. FacilityId: {FacilityId}, StartDate: {StartDate}, EndDate: {EndDate}, Page: {Page}, PageSize: {PageSize}",
                facilityId, startDate, endDate, page, pageSize);

            var query = _context.Appointments
                .Include(a => a.Patient)
                .Include(a => a.Clinic)
                .Where(a => a.FacilityId == facilityId && a.StartTime >= startDate && a.StartTime <= endDate);

            // Apply clinic filter
            if (clinicId.HasValue)
            {
                query = query.Where(a => a.ClinicId == clinicId.Value);
                _logger.LogInformation("Filter applied: ClinicId = {ClinicId}", clinicId.Value);
            }

            // Apply search filter
            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(a =>
                    a.Patient!.Name.Contains(search) ||
                    a.Patient!.Code.Contains(search) ||
                    (a.Patient!.Phone != null && a.Patient.Phone.Contains(search)));
                _logger.LogInformation("Search executed: Query = {Search}", search);
            }

            // Apply sorting
            query = sortBy.ToLower() switch
            {
                "time" => query.OrderBy(a => a.StartTime),
                "time_desc" => query.OrderByDescending(a => a.StartTime),
                "patient" => query.OrderBy(a => a.Patient!.Name),
                "patient_desc" => query.OrderByDescending(a => a.Patient!.Name),
                _ => query.OrderBy(a => a.StartTime)
            };

            var totalCount = await query.CountAsync();

            var appointments = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Get invoice information for each appointment (match by AppointmentId)
            var appointmentIds = appointments.Select(a => a.Id).ToList();
            var invoices = await _context.Invoices
                .Where(i => appointmentIds.Contains(i.AppointmentId ?? 0) && i.Status != "Draft")
                .ToListAsync();

            var records = appointments.Select(a =>
            {
                // Find invoice specifically for this appointment
                var appointmentInvoice = invoices.FirstOrDefault(i => i.AppointmentId == a.Id);
                return new RecordsResponse
                {
                    AppointmentId = a.Id,
                    PatientId = a.PatientId,
                    PatientName = a.Patient!.Name,
                    PatientCode = a.Patient.Code,
                    AppointmentTime = a.StartTime,
                    Status = a.Status,
                    ClinicName = a.Clinic!.Name,
                    WalletBalance = a.Patient.WalletBalance,
                    Currency = a.Patient.Currency,
                    InvoiceId = appointmentInvoice?.Id,
                    InvoiceStatus = appointmentInvoice?.Status
                };
            }).ToList();

            var result = new PagedResult<RecordsResponse>
            {
                Items = records,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };

            return Ok(result);
        }
    }
}

