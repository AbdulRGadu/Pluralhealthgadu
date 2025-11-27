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

                // Load patients - use search if search term provided, otherwise load all
                if (!string.IsNullOrWhiteSpace(patientSearch))
                {
                    // Filter patients based on search
                    var patients = await _apiClient.GetAsync<List<PatientOption>>($"/api/patients/search?q={Uri.EscapeDataString(patientSearch)}");
                    viewModel.Patients = patients ?? new List<PatientOption>();
                }
                else
                {
                    // Load all patients by default
                    var allPatients = await _apiClient.GetAsync<List<PatientOption>>("/api/patients/list");
                    viewModel.Patients = allPatients ?? new List<PatientOption>();
                }

                // Load all appointments (sorted by date/time ascending)
                _logger.LogInformation("Fetching appointments list from API: /api/appointments/list");
                var appointments = await _apiClient.GetAsync<List<AppointmentListItem>>("/api/appointments/list");
                
                viewModel.Appointments = appointments ?? new List<AppointmentListItem>();
                _logger.LogInformation("Loaded {Count} appointments into view model", viewModel.Appointments.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading appointment form: {Message}", ex.Message);
                viewModel.ErrorMessage = "An error occurred while loading the form.";
                viewModel.Appointments = new List<AppointmentListItem>(); // Initialize empty list on error
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
                
                // Parse user-friendly error message from API response
                model.ErrorMessage = ParseApiErrorMessage(ex.Message);
                
                // Reload dropdowns, patients, and appointments list
                var clinics = await _apiClient.GetAsync<List<ClinicOption>>("/api/clinics");
                model.Clinics = clinics ?? new List<ClinicOption>();

                var appointmentTypes = await _apiClient.GetAsync<List<AppointmentTypeOption>>("/api/appointmenttypes");
                model.AppointmentTypes = appointmentTypes ?? new List<AppointmentTypeOption>();

                // Reload patients (all or filtered based on search)
                if (!string.IsNullOrWhiteSpace(model.PatientSearch))
                {
                    var patients = await _apiClient.GetAsync<List<PatientOption>>($"/api/patients/search?q={Uri.EscapeDataString(model.PatientSearch)}");
                    model.Patients = patients ?? new List<PatientOption>();
                }
                else
                {
                    var allPatients = await _apiClient.GetAsync<List<PatientOption>>("/api/patients/list");
                    model.Patients = allPatients ?? new List<PatientOption>();
                }

                var appointments = await _apiClient.GetAsync<List<AppointmentListItem>>("/api/appointments/list");
                model.Appointments = appointments ?? new List<AppointmentListItem>();

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating appointment");
                model.ErrorMessage = "An error occurred while creating the appointment. Please try again.";
                
                // Reload dropdowns, patients, and appointments list
                var clinics = await _apiClient.GetAsync<List<ClinicOption>>("/api/clinics");
                model.Clinics = clinics ?? new List<ClinicOption>();

                var appointmentTypes = await _apiClient.GetAsync<List<AppointmentTypeOption>>("/api/appointmenttypes");
                model.AppointmentTypes = appointmentTypes ?? new List<AppointmentTypeOption>();

                // Reload patients (all or filtered based on search)
                if (!string.IsNullOrWhiteSpace(model.PatientSearch))
                {
                    var patients = await _apiClient.GetAsync<List<PatientOption>>($"/api/patients/search?q={Uri.EscapeDataString(model.PatientSearch)}");
                    model.Patients = patients ?? new List<PatientOption>();
                }
                else
                {
                    var allPatients = await _apiClient.GetAsync<List<PatientOption>>("/api/patients/list");
                    model.Patients = allPatients ?? new List<PatientOption>();
                }

                var appointments = await _apiClient.GetAsync<List<AppointmentListItem>>("/api/appointments/list");
                model.Appointments = appointments ?? new List<AppointmentListItem>();

                return View(model);
            }
        }

        private string ParseApiErrorMessage(string errorMessage)
        {
            // Try to extract user-friendly message from API error response
            if (string.IsNullOrWhiteSpace(errorMessage))
                return "An error occurred while creating the appointment. Please try again.";

            // Check if it's a direct error message (from BadRequest with string)
            if (errorMessage.Contains("Please select") || errorMessage.Contains("not found") || 
                errorMessage.Contains("cannot be in the past"))
            {
                // Extract the message between quotes or after colon
                var colonIndex = errorMessage.LastIndexOf(':');
                if (colonIndex > 0)
                {
                    var message = errorMessage.Substring(colonIndex + 1).Trim();
                    // Remove JSON formatting if present
                    message = message.Replace("\"", "").Replace("{", "").Replace("}", "").Trim();
                    if (!string.IsNullOrWhiteSpace(message))
                        return message;
                }
            }

            // Check for validation errors in JSON format
            if (errorMessage.Contains("errors") || errorMessage.Contains("validation"))
            {
                // Common validation error messages
                if (errorMessage.Contains("patientId") || errorMessage.Contains("PatientId"))
                    return "Please select a patient. Search for a patient first and select them from the dropdown.";
                
                if (errorMessage.Contains("clinicId") || errorMessage.Contains("ClinicId"))
                    return "Please select a clinic.";
                
                if (errorMessage.Contains("appointmentTypeId") || errorMessage.Contains("AppointmentTypeId"))
                    return "Please select an appointment type.";
                
                if (errorMessage.Contains("request"))
                    return "Please fill in all required fields: Patient, Clinic, Appointment Type, Date, and Time.";
            }

            // Default fallback
            return "An error occurred while creating the appointment. Please ensure all fields are filled correctly and try again.";
        }
    }
}

