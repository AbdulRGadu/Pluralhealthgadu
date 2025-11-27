using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using pluralhealth_API.Data;
using pluralhealth_API.DTOs;
using pluralhealth_API.Models;

namespace pluralhealth_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PaymentsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<PaymentsController> _logger;

        public PaymentsController(ApplicationDbContext context, ILogger<PaymentsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpPost]
        public async Task<ActionResult<Payment>> ProcessPayment([FromBody] ProcessPaymentRequest request)
        {
            var facilityId = (int)(HttpContext.Items["FacilityId"] ?? 1);
            var userId = (int)(HttpContext.Items["UserId"] ?? 1);

            // Validation
            if (request.Amount <= 0)
                return BadRequest("Payment amount must be greater than 0.");

            // Get invoice with related data
            var invoice = await _context.Invoices
                .Include(i => i.Patient)
                .Include(i => i.Payments)
                .FirstOrDefaultAsync(i => i.Id == request.InvoiceId && i.FacilityId == facilityId);

            if (invoice == null)
                return NotFound("Invoice not found.");

            // Validate invoice state
            if (invoice.Status == "Draft")
                return BadRequest("Cannot pay a draft invoice. Please finalize it first.");

            if (invoice.Status == "Paid")
                return BadRequest("Invoice is already fully paid.");

            // Calculate total paid
            var totalPaid = invoice.Payments?.Sum(p => p.Amount) ?? 0;
            var remainingBalance = invoice.Total - totalPaid;

            if (request.Amount > remainingBalance)
                return BadRequest($"Payment amount exceeds remaining balance of {remainingBalance:C}.");

            // Validate wallet balance if paying via wallet
            if (request.PaymentMethod.Equals("Wallet", StringComparison.OrdinalIgnoreCase))
            {
                if (invoice.Patient == null)
                    return BadRequest("Patient information not found.");

                if (request.Amount > invoice.Patient.WalletBalance)
                {
                    _logger.LogWarning("Insufficient wallet funds. PatientId: {PatientId}, WalletBalance: {WalletBalance}, PaymentAmount: {Amount}",
                        invoice.PatientId, invoice.Patient.WalletBalance, request.Amount);
                    return BadRequest($"Insufficient wallet funds. Wallet balance: {invoice.Patient.WalletBalance:C}, Payment amount: {request.Amount:C}");
                }
            }

            // Idempotency check: Prevent duplicate payments within a short time window (5 seconds)
            // Check if an identical payment was just processed
            var recentPayment = await _context.Payments
                .Where(p => p.InvoiceId == request.InvoiceId &&
                    p.Amount == request.Amount &&
                    p.CreatedBy == userId &&
                    p.CreatedAt >= DateTime.Now.AddSeconds(-5))
                .FirstOrDefaultAsync();
            
            if (recentPayment != null)
            {
                _logger.LogWarning("Duplicate payment attempt detected. InvoiceId: {InvoiceId}, Amount: {Amount}, UserId: {UserId}",
                    request.InvoiceId, request.Amount, userId);
                return BadRequest("A payment with this amount was recently processed. Please refresh the page.");
            }

            // Create payment
            var payment = new Payment
            {
                InvoiceId = request.InvoiceId,
                Amount = Math.Round(request.Amount, 2),
                PaymentMethod = request.PaymentMethod,
                FacilityId = facilityId,
                CreatedBy = userId,
                CreatedAt = DateTime.Now
            };

            _context.Payments.Add(payment);

            // Update invoice status
            var newTotalPaid = totalPaid + request.Amount;
            if (newTotalPaid >= invoice.Total)
            {
                invoice.Status = "Paid";
            }
            else
            {
                invoice.Status = "PartiallyPaid";
            }

            // Update patient wallet balance (deduct payment only if paying via wallet)
            if (invoice.Patient != null && request.PaymentMethod.Equals("Wallet", StringComparison.OrdinalIgnoreCase))
            {
                invoice.Patient.WalletBalance -= request.Amount;
                _logger.LogInformation("Wallet balance deducted. PatientId: {PatientId}, Amount: {Amount}, NewBalance: {NewBalance}",
                    invoice.PatientId, request.Amount, invoice.Patient.WalletBalance);
            }

            // Update patient status to "Awaiting Vitals" after payment (full or partial)
            // Business rule: Move to Awaiting Vitals on any successful payment
            if (invoice.Patient != null)
            {
                invoice.Patient.Status = "Awaiting Vitals";
                _logger.LogInformation("Patient {PatientId} status updated to 'Awaiting Vitals' after payment for Invoice {InvoiceId}. Payment amount: {Amount}, Invoice status: {Status}",
                    invoice.PatientId, invoice.Id, request.Amount, invoice.Status);
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation("Payment processed. PaymentId: {PaymentId}, InvoiceId: {InvoiceId}, Amount: {Amount}, FacilityId: {FacilityId}, CreatedBy: {CreatedBy}",
                payment.Id, invoice.Id, request.Amount, facilityId, userId);

            return CreatedAtAction(nameof(GetPayment), new { id = payment.Id }, payment);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Payment>> GetPayment(int id)
        {
            var facilityId = (int)(HttpContext.Items["FacilityId"] ?? 1);

            var payment = await _context.Payments
                .Include(p => p.Invoice)
                .FirstOrDefaultAsync(p => p.Id == id && p.FacilityId == facilityId);

            if (payment == null)
                return NotFound();

            return Ok(payment);
        }

        [HttpGet("invoice/{invoiceId}")]
        public async Task<ActionResult<List<Payment>>> GetInvoicePayments(int invoiceId)
        {
            var facilityId = (int)(HttpContext.Items["FacilityId"] ?? 1);

            var invoice = await _context.Invoices
                .FirstOrDefaultAsync(i => i.Id == invoiceId && i.FacilityId == facilityId);

            if (invoice == null)
                return NotFound("Invoice not found.");

            var payments = await _context.Payments
                .Where(p => p.InvoiceId == invoiceId && p.FacilityId == facilityId)
                .OrderBy(p => p.CreatedAt)
                .ToListAsync();

            return Ok(payments);
        }
    }
}

