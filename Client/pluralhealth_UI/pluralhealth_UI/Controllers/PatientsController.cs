using Microsoft.AspNetCore.Mvc;
using pluralhealth_UI.Models.ViewModels;
using pluralhealth_UI.Services;

namespace pluralhealth_UI.Controllers
{
    public class PatientsController : Controller
    {
        private readonly ApiClient _apiClient;
        private readonly ILogger<PatientsController> _logger;

        public PatientsController(ApiClient apiClient, ILogger<PatientsController> logger)
        {
            _apiClient = apiClient;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Create()
        {
            var viewModel = new CreatePatientViewModel
            {
                Currency = "NGN"
            };
            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreatePatientViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var request = new
                {
                    name = model.Name,
                    phone = model.Phone,
                    walletBalance = model.WalletBalance,
                    currency = model.Currency
                };

                await _apiClient.PostAsync<object>("/api/patients", request);

                TempData["SuccessMessage"] = $"Patient created successfully!";
                return RedirectToAction("Index", "Records");
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Error creating patient");
                model.ErrorMessage = ex.Message.Contains("API Error") ? ex.Message : "An error occurred while creating the patient. Please try again.";
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating patient");
                model.ErrorMessage = "An error occurred while creating the patient. Please try again.";
                return View(model);
            }
        }
    }
}

