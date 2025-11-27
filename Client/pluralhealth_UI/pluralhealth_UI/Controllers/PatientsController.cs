using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using pluralhealth_UI.Models.ViewModels;
using pluralhealth_UI.Services;
using System.Net.Http;

namespace pluralhealth_UI.Controllers
{
    [Authorize]
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
        public async Task<IActionResult> Create()
        {
            var viewModel = new CreatePatientViewModel
            {
                Currency = "NGN"
            };

            try
            {
                // Fetch list of patients
                _logger.LogInformation("Fetching patients list from API: /api/patients/list");
                var patientsResponse = await _apiClient.GetAsync<List<PatientListItem>>("/api/patients/list");
                
                viewModel.Patients = patientsResponse ?? new List<PatientListItem>();
                _logger.LogInformation("Loaded {Count} patients into view model", viewModel.Patients.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading patients list: {Message}", ex.Message);
                viewModel.Patients = new List<PatientListItem>();
            }

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreatePatientViewModel model)
        {
            if (!ModelState.IsValid)
            {
                // Reload patients list even on validation error
                try
                {
                    var patientsResponse = await _apiClient.GetAsync<List<PatientListItem>>("/api/patients/list");
                    model.Patients = patientsResponse ?? new List<PatientListItem>();
                }
                catch
                {
                    model.Patients = new List<PatientListItem>();
                }
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
                return RedirectToAction("Create", "Patients");
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Error creating patient");
                model.ErrorMessage = ex.Message.Contains("API Error") ? ex.Message : "An error occurred while creating the patient. Please try again.";
                
                // Reload patients list
                try
                {
                    var patientsResponse = await _apiClient.GetAsync<List<PatientListItem>>("/api/patients/list");
                    model.Patients = patientsResponse ?? new List<PatientListItem>();
                }
                catch
                {
                    model.Patients = new List<PatientListItem>();
                }
                
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating patient");
                model.ErrorMessage = "An error occurred while creating the patient. Please try again.";
                
                // Reload patients list
                try
                {
                    var patientsResponse = await _apiClient.GetAsync<List<PatientListItem>>("/api/patients/list");
                    model.Patients = patientsResponse ?? new List<PatientListItem>();
                }
                catch
                {
                    model.Patients = new List<PatientListItem>();
                }
                
                return View(model);
            }
        }
    }
}

