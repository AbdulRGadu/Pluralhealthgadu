using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using pluralhealth_API.Data;
using pluralhealth_API.DTOs;
using pluralhealth_API.Models;

namespace pluralhealth_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class InvoicesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<InvoicesController> _logger;

        public InvoicesController(ApplicationDbContext context, ILogger<InvoicesController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet("patient/{patientId}")]
        public async Task<ActionResult<List<Invoice>>> GetPatientInvoices(int patientId)
        {
            var facilityId = (int)(HttpContext.Items["FacilityId"] ?? 1);

            var invoices = await _context.Invoices
                .Include(i => i.Items)
                .Include(i => i.Payments)
                .Where(i => i.PatientId == patientId && i.FacilityId == facilityId)
                .OrderByDescending(i => i.CreatedAt)
                .ToListAsync();

            return Ok(invoices);
        }

        [HttpPost]
        public async Task<ActionResult<Invoice>> CreateInvoice([FromBody] CreateInvoiceRequest request)
        {
            var facilityId = (int)(HttpContext.Items["FacilityId"] ?? 1);
            var userId = (int)(HttpContext.Items["UserId"] ?? 1);

            // Validation
            if (request.PatientId <= 0)
                return BadRequest("PatientId is required.");

            if (request.Items == null || request.Items.Count == 0)
                return BadRequest("At least one invoice item is required.");

            // Verify patient exists
            var patient = await _context.Patients
                .FirstOrDefaultAsync(p => p.Id == request.PatientId && p.FacilityId == facilityId);
            if (patient == null)
                return NotFound("Patient not found.");

            // Verify appointment exists and belongs to patient if provided
            if (request.AppointmentId.HasValue)
            {
                var appointment = await _context.Appointments
                    .FirstOrDefaultAsync(a => a.Id == request.AppointmentId.Value && 
                        a.PatientId == request.PatientId && 
                        a.FacilityId == facilityId);
                if (appointment == null)
                    return NotFound("Appointment not found or does not belong to this patient.");

                // Idempotency check: Prevent creating duplicate invoices for the same appointment
                // (unless the existing invoice is Draft and can be edited)
                var existingInvoice = await _context.Invoices
                    .FirstOrDefaultAsync(i => i.AppointmentId == request.AppointmentId.Value && 
                        i.PatientId == request.PatientId && 
                        i.FacilityId == facilityId &&
                        i.Status != "Draft"); // Allow editing Draft invoices
                
                if (existingInvoice != null)
                {
                    return BadRequest($"An invoice already exists for this appointment (Invoice ID: {existingInvoice.Id}).");
                }
            }

            // Calculate totals
            decimal subtotal = 0;
            var invoiceItems = new List<InvoiceItem>();

            foreach (var itemRequest in request.Items)
            {
                if (itemRequest.Quantity <= 0)
                    return BadRequest("Quantity must be greater than 0.");

                if (itemRequest.UnitPrice < 0)
                    return BadRequest("Unit price cannot be negative.");

                if (itemRequest.DiscountAmount < 0)
                    return BadRequest("Discount amount cannot be negative.");

                var lineTotal = (itemRequest.Quantity * itemRequest.UnitPrice) - itemRequest.DiscountAmount;
                if (lineTotal < 0)
                    return BadRequest("Line total cannot be negative.");

                subtotal += lineTotal;

                invoiceItems.Add(new InvoiceItem
                {
                    ServiceName = itemRequest.ServiceName,
                    Quantity = itemRequest.Quantity,
                    UnitPrice = itemRequest.UnitPrice,
                    DiscountAmount = itemRequest.DiscountAmount,
                    LineTotal = Math.Round(lineTotal, 2)
                });
            }

            if (request.DiscountAmount < 0)
                return BadRequest("Invoice discount cannot be negative.");

            var total = Math.Round(subtotal - request.DiscountAmount, 2);
            if (total < 0)
                return BadRequest("Invoice total cannot be negative.");

            var invoice = new Invoice
            {
                PatientId = request.PatientId,
                AppointmentId = request.AppointmentId,
                Status = "Draft",
                Subtotal = Math.Round(subtotal, 2),
                DiscountAmount = Math.Round(request.DiscountAmount, 2),
                Total = total,
                FacilityId = facilityId,
                CreatedBy = userId,
                CreatedAt = DateTime.Now,
                Items = invoiceItems
            };

            _context.Invoices.Add(invoice);
            await _context.SaveChangesAsync();

            // Log discount if applied
            if (request.DiscountAmount > 0)
            {
                _logger.LogInformation("Discount applied on invoice creation. InvoiceId: {InvoiceId}, DiscountAmount: {DiscountAmount}, FacilityId: {FacilityId}, CreatedBy: {CreatedBy}",
                    invoice.Id, request.DiscountAmount, facilityId, userId);
            }

            // Log line-level discounts
            foreach (var item in invoiceItems.Where(i => i.DiscountAmount > 0))
            {
                _logger.LogInformation("Line discount applied. InvoiceId: {InvoiceId}, ServiceName: {ServiceName}, DiscountAmount: {DiscountAmount}, FacilityId: {FacilityId}",
                    invoice.Id, item.ServiceName, item.DiscountAmount, facilityId);
            }

            _logger.LogInformation("Invoice created. InvoiceId: {InvoiceId}, PatientId: {PatientId}, FacilityId: {FacilityId}, CreatedBy: {CreatedBy}",
                invoice.Id, invoice.PatientId, facilityId, userId);

            return CreatedAtAction(nameof(GetInvoice), new { id = invoice.Id }, invoice);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Invoice>> GetInvoice(int id)
        {
            var facilityId = (int)(HttpContext.Items["FacilityId"] ?? 1);

            var invoice = await _context.Invoices
                .Include(i => i.Items)
                .Include(i => i.Payments)
                .Include(i => i.Patient)
                .FirstOrDefaultAsync(i => i.Id == id && i.FacilityId == facilityId);

            if (invoice == null)
                return NotFound();

            return Ok(invoice);
        }

        [HttpPut("{id}/finalize")]
        public async Task<ActionResult<Invoice>> FinalizeInvoice(int id)
        {
            var facilityId = (int)(HttpContext.Items["FacilityId"] ?? 1);

            var invoice = await _context.Invoices
                .Include(i => i.Items)
                .FirstOrDefaultAsync(i => i.Id == id && i.FacilityId == facilityId);

            if (invoice == null)
                return NotFound();

            if (invoice.Status != "Draft")
                return BadRequest("Only draft invoices can be finalized.");

            // Generate invoice number
            var invoiceCount = await _context.Invoices.CountAsync(i => i.FacilityId == facilityId && i.InvoiceNumber != null);
            invoice.InvoiceNumber = $"INV-{facilityId}-{DateTime.Now:yyyyMMdd}-{invoiceCount + 1:D4}";
            invoice.Status = "Finalized";
            invoice.FinalizedAt = DateTime.Now;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Invoice finalized. InvoiceId: {InvoiceId}, InvoiceNumber: {InvoiceNumber}, FacilityId: {FacilityId}",
                invoice.Id, invoice.InvoiceNumber, facilityId);

            return Ok(invoice);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<Invoice>> UpdateInvoice(int id, [FromBody] CreateInvoiceRequest request)
        {
            var facilityId = (int)(HttpContext.Items["FacilityId"] ?? 1);

            var invoice = await _context.Invoices
                .Include(i => i.Items)
                .FirstOrDefaultAsync(i => i.Id == id && i.FacilityId == facilityId);

            if (invoice == null)
                return NotFound();

            if (invoice.Status != "Draft")
                return BadRequest("Only draft invoices can be updated.");

            // Remove existing items
            _context.InvoiceItems.RemoveRange(invoice.Items);

            // Calculate new totals
            decimal subtotal = 0;
            var invoiceItems = new List<InvoiceItem>();

            foreach (var itemRequest in request.Items)
            {
                if (itemRequest.Quantity <= 0 || itemRequest.UnitPrice < 0 || itemRequest.DiscountAmount < 0)
                    return BadRequest("Invalid item data.");

                var lineTotal = (itemRequest.Quantity * itemRequest.UnitPrice) - itemRequest.DiscountAmount;
                if (lineTotal < 0)
                    return BadRequest("Line total cannot be negative.");

                subtotal += lineTotal;

                invoiceItems.Add(new InvoiceItem
                {
                    InvoiceId = invoice.Id,
                    ServiceName = itemRequest.ServiceName,
                    Quantity = itemRequest.Quantity,
                    UnitPrice = itemRequest.UnitPrice,
                    DiscountAmount = itemRequest.DiscountAmount,
                    LineTotal = Math.Round(lineTotal, 2)
                });
            }

            var total = Math.Round(subtotal - request.DiscountAmount, 2);
            if (total < 0)
                return BadRequest("Invoice total cannot be negative.");

            var oldDiscountAmount = invoice.DiscountAmount;
            invoice.Subtotal = Math.Round(subtotal, 2);
            invoice.DiscountAmount = Math.Round(request.DiscountAmount, 2);
            invoice.Total = total;
            invoice.Items = invoiceItems;

            await _context.SaveChangesAsync();

            // Log discount changes if applied
            if (request.DiscountAmount > 0 && request.DiscountAmount != oldDiscountAmount)
            {
                _logger.LogInformation("Discount applied on invoice update. InvoiceId: {InvoiceId}, OldDiscountAmount: {OldDiscountAmount}, NewDiscountAmount: {DiscountAmount}, FacilityId: {FacilityId}",
                    invoice.Id, oldDiscountAmount, request.DiscountAmount, facilityId);
            }

            // Log line-level discounts
            foreach (var item in invoiceItems.Where(i => i.DiscountAmount > 0))
            {
                _logger.LogInformation("Line discount applied on invoice update. InvoiceId: {InvoiceId}, ServiceName: {ServiceName}, DiscountAmount: {DiscountAmount}, FacilityId: {FacilityId}",
                    invoice.Id, item.ServiceName, item.DiscountAmount, facilityId);
            }

            _logger.LogInformation("Invoice updated. InvoiceId: {InvoiceId}, FacilityId: {FacilityId}", invoice.Id, facilityId);

            return Ok(invoice);
        }
    }
}

