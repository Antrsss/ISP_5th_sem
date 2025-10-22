using System.Text;
using System.Text.Json;
using WEB_353502_ZGIRSKAYA.Domain.Entities;
using WEB_353502_ZGIRSKAYA.Domain.Models;
using WEB_353502_ZGIRSKAYA.UI.Services.CocktailService;

namespace WEB_353502_ZGIRSKAYA.UI.Services
{
    public class ApiCocktailService : ICocktailService
    {
        private readonly HttpClient _httpClient;
        private readonly string _pageSize;
        private readonly JsonSerializerOptions _serializerOptions;

        public ApiCocktailService(HttpClient httpClient, IConfiguration configuration, ILogger<ApiCocktailService> logger)
        {
            _httpClient = httpClient;
            _pageSize = configuration.GetValue<string>("ItemsPerPage") ?? "3";
            _serializerOptions = new JsonSerializerOptions()
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles
            };
        }

        public async Task<ResponseData<ListModel<Cocktail>>> GetCocktailListAsync(string? categoryNormalizedName, int pageNo = 1)
        {
            var queryParams = new List<string>();

            var urlString = string.IsNullOrEmpty(categoryNormalizedName)
                ? $"{_httpClient.BaseAddress!.AbsoluteUri}"
                : $"{_httpClient.BaseAddress!.AbsoluteUri}{categoryNormalizedName}";

            if (pageNo > 1)
            {
                queryParams.Add($"pageNo={pageNo}");
            }

            if (!_pageSize.Equals("3"))
            {
                queryParams.Add($"pageSize={_pageSize}");
            }

            if (queryParams.Any())
            {
                urlString += "?" + string.Join("&", queryParams);
            }

            var response = await _httpClient.GetAsync(new Uri(urlString));

            if (response.IsSuccessStatusCode)
            {
                try
                {
                    return await response.Content.ReadFromJsonAsync<ResponseData<ListModel<Cocktail>>>(_serializerOptions);
                }
                catch (JsonException ex)
                {
                    return ResponseData<ListModel<Cocktail>>.Error($"Ошибка: {ex.Message}");
                }
            }

            return ResponseData<ListModel<Cocktail>>.Error($"Данные не получены от сервера. Error: {response.StatusCode}");
        }

        public async Task<ResponseData<Cocktail>> GetCocktailByIdAsync(int id)
        {
            var response = await _httpClient.GetAsync($"{id}");

            if (response.IsSuccessStatusCode)
            {
                try
                {
                    return await response.Content.ReadFromJsonAsync<ResponseData<Cocktail>>(_serializerOptions);
                }
                catch (JsonException ex)
                {
                    return ResponseData<Cocktail>.Error($"Ошибка: {ex.Message}");
                }
            }

            return ResponseData<Cocktail>.Error($"Данные не получены от сервера. Error: {response.StatusCode}");
        }

        public async Task<ResponseData<Cocktail>> CreateCocktailAsync(Cocktail cocktail, IFormFile? formFile)
        {
            cocktail.PathToPicture = "images/noimage.jpg";

            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = _httpClient.BaseAddress
            };

            var content = new MultipartFormDataContent();

            if (formFile != null)
            {
                var streamContent = new StreamContent(formFile.OpenReadStream());
                content.Add(streamContent, "file", formFile.FileName);
            }

            var cocktailForSerialization = new
            {
                id = cocktail.Id,
                name = cocktail.Name,
                description = cocktail.Description,
                price = cocktail.Price,
                category = cocktail.Category,
                pathToPicture = cocktail.PathToPicture,
                mimeType = cocktail.MimeType
            };

            var cocktailJson = JsonSerializer.Serialize(cocktailForSerialization, _serializerOptions);

            var cocktailContent = new StringContent(cocktailJson, Encoding.UTF8, "application/json");
            content.Add(cocktailContent, "cocktail");

            request.Content = content;

            var response = await _httpClient.SendAsync(request, CancellationToken.None);

            var responseContentString = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                try
                {
                    var responseData = JsonSerializer.Deserialize<ResponseData<Cocktail>>(responseContentString, _serializerOptions);

                    return responseData ?? ResponseData<Cocktail>.Error("Пустой ответ от сервера");
                }
                catch (JsonException ex)
                {
                    return ResponseData<Cocktail>.Error($"Ошибка десериализации: {ex.Message}");
                }
            }

            return ResponseData<Cocktail>.Error($"Объект не добавлен. Error: {response.StatusCode}");
        }

        public async Task UpdateCocktailAsync(int id, Cocktail cocktail, IFormFile? formFile)
        {
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Put,
                RequestUri = new Uri(_httpClient.BaseAddress!, $"{id}")
            };

            var content = new MultipartFormDataContent();

            if (formFile != null)
            {
                var streamContent = new StreamContent(formFile.OpenReadStream());
                streamContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(formFile.ContentType);
                content.Add(streamContent, "file", formFile.FileName);
            }

            var cocktailForSerialization = new
            {
                id = cocktail.Id,
                name = cocktail.Name,
                description = cocktail.Description,
                price = cocktail.Price,
                category = cocktail.Category,
                pathToPicture = cocktail.PathToPicture,
                mimeType = cocktail.MimeType
            };

            var cocktailJson = JsonSerializer.Serialize(cocktailForSerialization, _serializerOptions);

            var cocktailContent = new StringContent(cocktailJson, Encoding.UTF8, "application/json");
            content.Add(cocktailContent, "cocktail");

            request.Content = content;

            var response = await _httpClient.SendAsync(request, CancellationToken.None);

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Объект не обновлен. Error: {response.StatusCode}");
            }

            var responseContent = await response.Content.ReadAsStringAsync();
        }

        public async Task DeleteCocktailAsync(int id)
        {
            var response = await _httpClient.DeleteAsync($"{id}");

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Объект не удален. Error: {response.StatusCode}");
            }
        }
    }
}