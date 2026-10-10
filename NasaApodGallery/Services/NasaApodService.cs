using System.Globalization;
using System.Text.Json;
using System.Text.Json.Nodes;
using NasaApodGallery.Models;

namespace NasaApodGallery.Services
{
    public class NasaApodService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<NasaApodService> _logger;

        private static readonly string[] ImageExtensions = { ".jpg", ".jpeg", ".png", ".gif" };

        public NasaApodService(HttpClient httpClient, IConfiguration configuration, ILogger<NasaApodService> logger)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<List<ApodDto>> GetApodRangeAsync(string startDate, string endDate)
        {
            var baseUrl = _configuration["NasaApi:BaseUrl"];
            var apiKey = _configuration["NasaApi:ApiKey"];
            var requestUrl = $"{baseUrl}?api_key={apiKey}&start_date={startDate}&end_date={endDate}";

            var response = await _httpClient.GetAsync(requestUrl);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsByteArrayAsync();

            var apods = JsonSerializer.Deserialize<List<ApodDto>>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? new List<ApodDto>();

            var valid = apods.Where(IsValidApod).ToList();

            var skipped = apods.Count - valid.Count;
            if (skipped > 0)
                _logger.LogWarning("Skipped {Skipped} of {Total} APOD entries with invalid data.", skipped, apods.Count);

            // APOD is returning junk right now, so fall back to NASA's Image and Video Library
            if (valid.Count == 0)
            {
                _logger.LogWarning("APOD returned no valid entries. Falling back to the NASA Image Library.");
                return await GetFromImageLibraryAsync();
            }

            return valid;
        }

        private async Task<List<ApodDto>> GetFromImageLibraryAsync()
        {
            var query = _configuration["NasaApi:FallbackQuery"] ?? "nebula";
            var url = $"https://images-api.nasa.gov/search?q={Uri.EscapeDataString(query)}&media_type=image";

            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            var items = JsonNode.Parse(json)?["collection"]?["items"]?.AsArray();

            var results = new List<ApodDto>();
            if (items == null) return results;

            foreach (var item in items)
            {
                var data = item?["data"]?[0];
                var links = item?["links"]?.AsArray();
                if (data == null || links == null) continue;

                var title = data["title"]?.GetValue<string>();
                var nasaId = data["nasa_id"]?.GetValue<string>();
                var created = data["date_created"]?.GetValue<string>();
                var description = data["description"]?.GetValue<string>() ?? string.Empty;

                if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(created)) continue;

                // Skip entries whose "title" is just a file code, like ARC-2010-ACD10-0054-011
                if (title == nasaId) continue;

                if (!DateTimeOffset.TryParse(created, CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsed))
                    continue;

                string? Pick(string marker) => links
                    .Select(l => l?["href"]?.GetValue<string>())
                    .FirstOrDefault(h => h != null && h.Contains(marker));

                var imageUrl = Pick("~medium.") ?? Pick("~small.") ?? Pick("~thumb.") ?? Pick("~orig.");
                if (imageUrl == null) continue;

                if (description.Length > 1500)
                    description = description.Substring(0, 1500) + "...";

                // NOTE: property names/types below must match your ApodDto.
                results.Add(new ApodDto
                {
                    Date = parsed.ToString("yyyy-MM-dd"),
                    Title = title,
                    Explanation = description,
                    Url = imageUrl,
                    MediaType = "image",
                    ServiceVersion = "v1"
                });
            }

            // One entry per date, in case your table treats the date as unique
            return results
                .GroupBy(r => r.Date)
                .Select(g => g.First())
                .Take(12)
                .ToList();
        }

        private static bool IsValidApod(ApodDto apod)
        {
            if (string.IsNullOrWhiteSpace(apod.Url) || string.IsNullOrWhiteSpace(apod.Title))
                return false;

            // The bad data NASA is returning right now
            if (apod.Title == "NASA Science") return false;
            if (apod.Url.Contains("nasa-logo", StringComparison.OrdinalIgnoreCase)) return false;
            if (apod.Url.Contains("wp-content/themes", StringComparison.OrdinalIgnoreCase)) return false;

            // Videos only need a valid link, not an image extension
            if (apod.MediaType != "image") return true;

            // Images must look like real image files
            var path = new Uri(apod.Url, UriKind.RelativeOrAbsolute).IsAbsoluteUri
                ? new Uri(apod.Url).AbsolutePath
                : apod.Url;

            return ImageExtensions.Any(ext => path.EndsWith(ext, StringComparison.OrdinalIgnoreCase));
        }
    }
}