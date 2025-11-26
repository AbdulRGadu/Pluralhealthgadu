using System.Text;
using System.Text.Json;

namespace pluralhealth_UI.Services
{
    public class ApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<ApiClient> _logger;
        private readonly string _baseUrl;

        public ApiClient(HttpClient httpClient, IConfiguration configuration, ILogger<ApiClient> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
            _baseUrl = configuration["ApiSettings:BaseUrl"] ?? "https://localhost:7031";
            _httpClient.BaseAddress = new Uri(_baseUrl);
            _httpClient.DefaultRequestHeaders.Add("X-Facility-Id", "1");
            _httpClient.DefaultRequestHeaders.Add("X-User-Id", "1");
        }

        public async Task<T?> GetAsync<T>(string endpoint)
        {
            try
            {
                var response = await _httpClient.GetAsync(endpoint);
                response.EnsureSuccessStatusCode();
                var content = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<T>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                return result;
            }
            catch (HttpRequestException)
            {
                _logger.LogWarning("HTTP error calling GET {Endpoint}", endpoint);
                return default(T);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling GET {Endpoint}", endpoint);
                throw;
            }
        }

        public async Task<T?> PostAsync<T>(string endpoint, object data)
        {
            try
            {
                var json = JsonSerializer.Serialize(data);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync(endpoint, content);
                
                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning("HTTP error calling POST {Endpoint}: {StatusCode} - {Error}", endpoint, response.StatusCode, errorContent);
                    throw new HttpRequestException($"API Error ({response.StatusCode}): {errorContent}");
                }
                
                var responseContent = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<T>(responseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            }
            catch (HttpRequestException)
            {
                throw; // Re-throw HTTP exceptions with error messages
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling POST {Endpoint}", endpoint);
                throw;
            }
        }

        public async Task<T?> PutAsync<T>(string endpoint, object? data = null)
        {
            try
            {
                HttpContent? content = null;
                if (data != null)
                {
                    var json = JsonSerializer.Serialize(data);
                    content = new StringContent(json, Encoding.UTF8, "application/json");
                }
                var response = await _httpClient.PutAsync(endpoint, content);
                response.EnsureSuccessStatusCode();
                var responseContent = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<T>(responseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling PUT {Endpoint}", endpoint);
                throw;
            }
        }
    }
}

