using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using pluralhealth_UI.Models.ViewModels;
using pluralhealth_UI.Services;

namespace pluralhealth_UI.Controllers
{
    [Authorize]
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
        public async Task<IActionResult> Create(int patientId, int? appointmentId = null)
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

                var viewModel = new CreateInvoiceViewModel
                {
                    PatientId = patientId,
                    AppointmentId = appointmentId,
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
                    appointmentId = model.AppointmentId,
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
                await _apiClient.PutAsync<object>($"/api/invoices/{invoiceId}/finalize", null);

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

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var invoice = await _apiClient.GetAsync<InvoiceDetailResponse>($"/api/invoices/{id}");

                if (invoice == null)
                {
                    TempData["ErrorMessage"] = "Invoice not found.";
                    return RedirectToAction("Index", "Records");
                }

                if (invoice.Status != "Draft")
                {
                    TempData["ErrorMessage"] = "Only draft invoices can be edited.";
                    return RedirectToAction("Index", "Records");
                }

                var viewModel = new CreateInvoiceViewModel
                {
                    InvoiceId = invoice.Id,
                    PatientId = invoice.Patient.Id,
                    AppointmentId = invoice.AppointmentId,
                    PatientName = invoice.Patient.Name,
                    PatientCode = invoice.Patient.Code,
                    WalletBalance = invoice.Patient.WalletBalance,
                    Currency = invoice.Patient.Currency ?? "NGN",
                    Items = invoice.Items?.Select(i => new InvoiceItemViewModel
                    {
                        ServiceName = i.ServiceName,
                        Quantity = i.Quantity,
                        UnitPrice = i.UnitPrice,
                        DiscountAmount = i.DiscountAmount
                    }).ToList() ?? new List<InvoiceItemViewModel> { new InvoiceItemViewModel() },
                    DiscountAmount = invoice.DiscountAmount
                };

                return View("Create", viewModel); // Reuse Create view
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading invoice for edit");
                TempData["ErrorMessage"] = "An error occurred while loading the invoice.";
                return RedirectToAction("Index", "Records");
            }
        }

        [HttpPost]
        public async Task<IActionResult> Edit(CreateInvoiceViewModel model)
        {
            if (!ModelState.IsValid || !model.InvoiceId.HasValue)
            {
                return View("Create", model);
            }

            try
            {
                var request = new
                {
                    patientId = model.PatientId,
                    appointmentId = model.AppointmentId,
                    items = model.Items.Select(i => new
                    {
                        serviceName = i.ServiceName,
                        quantity = i.Quantity,
                        unitPrice = i.UnitPrice,
                        discountAmount = i.DiscountAmount
                    }).ToList(),
                    discountAmount = model.DiscountAmount
                };

                await _apiClient.PutAsync<object>($"/api/invoices/{model.InvoiceId.Value}", request);

                TempData["SuccessMessage"] = "Invoice updated successfully!";
                return RedirectToAction("Index", "Records");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating invoice");
                model.ErrorMessage = "An error occurred while updating the invoice. Please try again.";
                return View("Create", model);
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

        private class InvoiceDetailResponse
        {
            public int Id { get; set; }
            public int? AppointmentId { get; set; }
            public string Status { get; set; } = string.Empty;
            public decimal DiscountAmount { get; set; }
            public PatientDetailInfo Patient { get; set; } = new();
            public List<InvoiceItemDetail> Items { get; set; } = new();
        }

        private class PatientDetailInfo
        {
            public int Id { get; set; }
            public string Name { get; set; } = string.Empty;
            public string Code { get; set; } = string.Empty;
            public decimal WalletBalance { get; set; }
            public string? Currency { get; set; }
        }

        private class InvoiceItemDetail
        {
            public string ServiceName { get; set; } = string.Empty;
            public int Quantity { get; set; }
            public decimal UnitPrice { get; set; }
            public decimal DiscountAmount { get; set; }
        }
    }
}

