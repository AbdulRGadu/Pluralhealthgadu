using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using pluralhealth_UI.Models.ViewModels;
using pluralhealth_UI.Services;

namespace pluralhealth_UI.Controllers
{
    [Authorize]
    public class RecordsController : Controller
    {
        private readonly ApiClient _apiClient;
        private readonly ILogger<RecordsController> _logger;

        public RecordsController(ApiClient apiClient, ILogger<RecordsController> logger)
        {
            _apiClient = apiClient;
            _logger = logger;
        }

        public async Task<IActionResult> Index(
            DateTime? startDate,
            DateTime? endDate,
            int? clinicId,
            string? search,
            int page = 1,
            int pageSize = 20)
        {
            // Default to today only (using local time)
            var localStartDate = startDate ?? DateTime.Today;
            var localEndDate = endDate ?? DateTime.Today.AddDays(1).AddTicks(-1); // End of today
            
            var viewModel = new RecordsIndexViewModel
            {
                Page = page,
                PageSize = pageSize,
                StartDate = localStartDate,
                EndDate = localEndDate,
                ClinicId = clinicId,
                Search = search
            };

            try
            {
                // Build query string (using local time, no UTC conversion)
                var queryParams = new List<string>
                {
                    $"page={page}",
                    $"pageSize={pageSize}",
                    $"startDate={localStartDate:yyyy-MM-ddTHH:mm:ss}",
                    $"endDate={localEndDate:yyyy-MM-ddTHH:mm:ss}"
                };

                if (clinicId.HasValue)
                    queryParams.Add($"clinicId={clinicId.Value}");

                if (!string.IsNullOrWhiteSpace(search))
                    queryParams.Add($"search={Uri.EscapeDataString(search)}");

                var queryString = string.Join("&", queryParams);
                var response = await _apiClient.GetAsync<PagedRecordsResponse>($"/api/records?{queryString}");

                if (response != null)
                {
                    viewModel.TotalCount = response.TotalCount;
                    viewModel.TotalPages = response.TotalPages;

                    viewModel.Records = response.Items.Select(item => new RecordItem
                    {
                        AppointmentId = item.AppointmentId,
                        PatientId = item.PatientId,
                        PatientName = item.PatientName,
                        PatientCode = item.PatientCode,
                        AppointmentTime = item.AppointmentTime,
                        Status = item.Status,
                        ClinicName = item.ClinicName,
                        WalletBalance = item.WalletBalance,
                        Currency = item.Currency,
                        InvoiceId = item.InvoiceId,
                        InvoiceStatus = item.InvoiceStatus
                    }).ToList();
                }

                // Load clinics for dropdown
                var clinicsResponse = await _apiClient.GetAsync<List<ClinicOption>>("/api/clinics");
                viewModel.Clinics = clinicsResponse ?? new List<ClinicOption>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading records");
                viewModel.ErrorMessage = "An error occurred while loading records. Please try again.";
            }

            return View(viewModel);
        }

        private class PagedRecordsResponse
        {
            public List<RecordResponseItem> Items { get; set; } = new();
            public int TotalCount { get; set; }
            public int Page { get; set; }
            public int PageSize { get; set; }
            public int TotalPages { get; set; }
        }

        private class RecordResponseItem
        {
            public int AppointmentId { get; set; }
            public int PatientId { get; set; }
            public string PatientName { get; set; } = string.Empty;
            public string PatientCode { get; set; } = string.Empty;
            public DateTime AppointmentTime { get; set; }
            public string Status { get; set; } = string.Empty;
            public string ClinicName { get; set; } = string.Empty;
            public decimal WalletBalance { get; set; }
            public string Currency { get; set; } = string.Empty;
            public int? InvoiceId { get; set; }
            public string? InvoiceStatus { get; set; }
        }
    }
}

