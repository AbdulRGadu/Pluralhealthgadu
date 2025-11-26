using Microsoft.AspNetCore.Mvc;
using pluralhealth_UI.Models.ViewModels;
using pluralhealth_UI.Services;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace pluralhealth_UI.Controllers
{
    public class PaymentsController : Controller
    {
        private readonly ApiClient _apiClient;
        private readonly ILogger<PaymentsController> _logger;

        public PaymentsController(ApiClient apiClient, ILogger<PaymentsController> logger)
        {
            _apiClient = apiClient;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> Create(int invoiceId)
        {
            try
            {
                var invoiceResponse = await _apiClient.GetAsync<InvoiceDetailResponse>($"/api/invoices/{invoiceId}");

                if (invoiceResponse == null)
                {
                    TempData["ErrorMessage"] = "Invoice not found.";
                    return RedirectToAction("Index", "Records");
                }

                // Calculate total paid
                decimal totalPaid = invoiceResponse.Payments?.Sum(p => p.Amount) ?? 0;
                var remainingBalance = invoiceResponse.Total - totalPaid;

                var viewModel = new CreatePaymentViewModel
                {
                    InvoiceId = invoiceId,
                    InvoiceNumber = invoiceResponse.InvoiceNumber ?? "",
                    PatientName = invoiceResponse.Patient?.Name ?? "",
                    InvoiceTotal = invoiceResponse.Total,
                    TotalPaid = totalPaid,
                    RemainingBalance = remainingBalance,
                    Currency = invoiceResponse.Patient?.Currency ?? "NGN",
                    Amount = remainingBalance,
                    PaymentMethod = "Cash"
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading payment form");
                TempData["ErrorMessage"] = "An error occurred while loading the payment form.";
                return RedirectToAction("Index", "Records");
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreatePaymentViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var request = new
                {
                    invoiceId = model.InvoiceId,
                    amount = model.Amount,
                    paymentMethod = model.PaymentMethod
                };

                await _apiClient.PostAsync<object>("/api/payments", request);

                TempData["SuccessMessage"] = "Payment processed successfully! Patient moved to Awaiting Vitals.";
                return RedirectToAction("Index", "Records");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing payment");
                model.ErrorMessage = "An error occurred while processing the payment. Please try again.";
                return View(model);
            }
        }

        private class InvoiceDetailResponse
        {
            public int Id { get; set; }
            public string? InvoiceNumber { get; set; }
            public decimal Total { get; set; }
            public PatientInfo? Patient { get; set; }
            public List<PaymentInfo>? Payments { get; set; }
        }

        private class PatientInfo
        {
            public string Name { get; set; } = string.Empty;
            public string? Currency { get; set; }
        }

        private class PaymentInfo
        {
            public decimal Amount { get; set; }
        }
    }
}

