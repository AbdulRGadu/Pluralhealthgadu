using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using pluralhealth_UI.Models.ViewModels;
using pluralhealth_UI.Services;

namespace pluralhealth_UI.Controllers
{
    [Authorize(Roles = "SuperAdmin")]
    public class PaidInvoicesController : Controller
    {
        private readonly ApiClient _apiClient;
        private readonly ILogger<PaidInvoicesController> _logger;

        public PaidInvoicesController(ApiClient apiClient, ILogger<PaidInvoicesController> logger)
        {
            _apiClient = apiClient;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var viewModel = new PaidInvoicesViewModel();

            try
            {
                _logger.LogInformation("Fetching paid invoices from API");
                var invoices = await _apiClient.GetAsync<List<PaidInvoiceItem>>("/api/invoices/paid");
                
                viewModel.Invoices = invoices ?? new List<PaidInvoiceItem>();
                _logger.LogInformation("Loaded {Count} paid invoices", viewModel.Invoices.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading paid invoices: {Message}", ex.Message);
                viewModel.ErrorMessage = "An error occurred while loading paid invoices. Please try again.";
                viewModel.Invoices = new List<PaidInvoiceItem>();
            }

            return View(viewModel);
        }
    }
}

