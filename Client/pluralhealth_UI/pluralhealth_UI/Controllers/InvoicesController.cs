using Microsoft.AspNetCore.Mvc;
using pluralhealth_UI.Models.ViewModels;
using pluralhealth_UI.Services;
using System.Text.Json;

namespace pluralhealth_UI.Controllers
{
    public class InvoicesController : Controller
    {
        private readonly ApiClient _apiClient;
        private readonly ILogger<InvoicesController> _logger;

        public InvoicesController(ApiClient apiClient, ILogger<InvoicesController> logger)
        {
            _apiClient = apiClient;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> Create(int patientId)
        {
            try
            {
                // Get patient info
                var patient = await _apiClient.GetAsync<PatientInfo>($"/api/patients/{patientId}");

                if (patient == null)
                {
                    TempData["ErrorMessage"] = "Patient not found.";
                    return RedirectToAction("Index", "Records");
                }

                if (patient == null)
                {
                    TempData["ErrorMessage"] = "Patient not found.";
                    return RedirectToAction("Index", "Records");
                }

                var viewModel = new CreateInvoiceViewModel
                {
                    PatientId = patientId,
                    PatientName = patient.Name,
                    PatientCode = patient.Code,
                    WalletBalance = patient.WalletBalance,
                    Currency = patient.Currency ?? "NGN"
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading invoice form");
                TempData["ErrorMessage"] = "An error occurred while loading the form.";
                return RedirectToAction("Index", "Records");
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateInvoiceViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var request = new
                {
                    patientId = model.PatientId,
                    items = model.Items.Select(i => new
                    {
                        serviceName = i.ServiceName,
                        quantity = i.Quantity,
                        unitPrice = i.UnitPrice,
                        discountAmount = i.DiscountAmount
                    }).ToList(),
                    discountAmount = model.DiscountAmount
                };

                var invoiceResponse = await _apiClient.PostAsync<InvoiceResponse>("/api/invoices", request);
                
                if (invoiceResponse == null || invoiceResponse.Id == 0)
                {
                    model.ErrorMessage = "Failed to create invoice. Please try again.";
                    return View(model);
                }

                var invoiceId = invoiceResponse.Id;

                // Finalize the invoice
                await _apiClient.PutAsync<JsonElement>($"/api/invoices/{invoiceId}/finalize", null);

                TempData["SuccessMessage"] = "Invoice created and finalized successfully!";
                return RedirectToAction("Index", "Records");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating invoice");
                model.ErrorMessage = "An error occurred while creating the invoice. Please try again.";
                return View(model);
            }
        }

        private class PatientInfo
        {
            public int Id { get; set; }
            public string Name { get; set; } = string.Empty;
            public string Code { get; set; } = string.Empty;
            public decimal WalletBalance { get; set; }
            public string? Currency { get; set; }
        }

        private class InvoiceResponse
        {
            public int Id { get; set; }
        }
    }
}

