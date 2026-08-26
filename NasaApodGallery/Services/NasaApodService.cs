using System.Text.Json;
using NasaApodGallery.Models;

namespace NasaApodGallery.Services
{
    public class NasaApodService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public NasaApodService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
        }

        public async Task<List<ApodDto>> GetApodRangeAsync(string startDate,string endDate)
        {   var baseUrl = _configuration["NasaApi:BaseUrl"];
            var apiKey = _configuration["NasaApi:ApiKey"];
            var requestUrl = $"{baseUrl}?api_key={apiKey}&start_date={startDate}&end_date={endDate}";

            var response = await _httpClient.GetAsync(requestUrl);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsByteArrayAsync();

            var apods = JsonSerializer.Deserialize<List<ApodDto>>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            
            return apods ?? new List<ApodDto>();
        }
    }
}