using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using pluralhealth_UI.Models.ViewModels;
using pluralhealth_UI.Services;

namespace pluralhealth_UI.Controllers
{
    [Authorize]
    public class AppointmentsController : Controller
    {
        private readonly ApiClient _apiClient;
        private readonly ILogger<AppointmentsController> _logger;

        public AppointmentsController(ApiClient apiClient, ILogger<AppointmentsController> logger)
        {
            _apiClient = apiClient;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> Create(string? patientSearch = null)
        {
            var viewModel = new CreateAppointmentViewModel
            {
                PatientSearch = patientSearch
            };

            try
            {
                // Load clinics
                var clinics = await _apiClient.GetAsync<List<ClinicOption>>("/api/clinics");
                viewModel.Clinics = clinics ?? new List<ClinicOption>();

                // Load appointment types
                var appointmentTypesResponse = await _apiClient.GetAsync<List<AppointmentTypeOption>>("/api/appointmenttypes");
                viewModel.AppointmentTypes = appointmentTypesResponse ?? new List<AppointmentTypeOption>();

                // Search patients if search term provided
                if (!string.IsNullOrWhiteSpace(patientSearch))
                {
                    var patients = await _apiClient.GetAsync<List<PatientOption>>($"/api/patients/search?q={Uri.EscapeDataString(patientSearch)}");
                    viewModel.Patients = patients ?? new List<PatientOption>();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading appointment form");
                viewModel.ErrorMessage = "An error occurred while loading the form.";
            }

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateAppointmentViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                // Combine date and time (using local time, no UTC conversion)
                var startTime = model.AppointmentDate.Date.Add(model.AppointmentTime);

                var request = new
                {
                    patientId = model.PatientId,
                    clinicId = model.ClinicId,
                    appointmentTypeId = model.AppointmentTypeId,
                    startTime = startTime,
                    durationMinutes = model.DurationMinutes
                };

                await _apiClient.PostAsync<object>("/api/appointments", request);

                TempData["SuccessMessage"] = "Appointment created successfully!";
                return RedirectToAction("Index", "Records");
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Error creating appointment");
                // Extract error message from exception
                model.ErrorMessage = ex.Message.Contains("API Error") ? ex.Message : "An error occurred while creating the appointment. Please try again.";
                
                // Reload dropdowns
                var clinics = await _apiClient.GetAsync<List<ClinicOption>>("/api/clinics");
                model.Clinics = clinics ?? new List<ClinicOption>();

                var appointmentTypes = await _apiClient.GetAsync<List<AppointmentTypeOption>>("/api/appointmenttypes");
                model.AppointmentTypes = appointmentTypes ?? new List<AppointmentTypeOption>();

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating appointment");
                model.ErrorMessage = "An error occurred while creating the appointment. Please try again.";
                
                // Reload dropdowns
                var clinics = await _apiClient.GetAsync<List<ClinicOption>>("/api/clinics");
                model.Clinics = clinics ?? new List<ClinicOption>();

                var appointmentTypes = await _apiClient.GetAsync<List<AppointmentTypeOption>>("/api/appointmenttypes");
                model.AppointmentTypes = appointmentTypes ?? new List<AppointmentTypeOption>();

                return View(model);
            }
        }
    }
}

