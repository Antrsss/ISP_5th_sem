using System.Text.Json;
using Microsoft.Extensions.Logging;
using WEB_353502_ZGIRSKAYA.Domain.Entities;
using WEB_353502_ZGIRSKAYA.Domain.Models;
using WEB_353502_ZGIRSKAYA.UI.Services.CocktailCategoryService;

namespace WEB_353502_ZGIRSKAYA.UI.Services
{
    public class ApiCategoryService : ICategoryService
    {
        private readonly HttpClient _httpClient;
        private readonly JsonSerializerOptions _serializerOptions;
        private readonly ILogger<ApiCategoryService> _logger;

        public ApiCategoryService(HttpClient httpClient, ILogger<ApiCategoryService> logger)
        {
            _httpClient = httpClient;
            _serializerOptions = new JsonSerializerOptions()
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
            _logger = logger;
        }

        public async Task<ResponseData<List<CocktailCategory>>> GetCategoryListAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("");

                if (response.IsSuccessStatusCode)
                {
                    try
                    {
                        // API возвращает List<CocktailCategory>, а не ResponseData<List<CocktailCategory>>
                        var categories = await response.Content.ReadFromJsonAsync<List<CocktailCategory>>(_serializerOptions);
                        return ResponseData<List<CocktailCategory>>.Success(categories ?? new List<CocktailCategory>());
                    }
                    catch (JsonException ex)
                    {
                        _logger.LogError($"Ошибка десериализации: {ex.Message}");
                        return ResponseData<List<CocktailCategory>>.Error($"Ошибка: {ex.Message}");
                    }
                }

                _logger.LogError($"Данные не получены от сервера. Status: {response.StatusCode}");
                return ResponseData<List<CocktailCategory>>.Error($"Данные не получены от сервера. Error: {response.StatusCode}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Ошибка подключения: {ex.Message}");
                return ResponseData<List<CocktailCategory>>.Error($"Ошибка подключения: {ex.Message}");
            }
        }
    }
}