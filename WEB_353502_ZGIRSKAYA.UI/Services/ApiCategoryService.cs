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
            var response = await _httpClient.GetAsync("");

            if (response.IsSuccessStatusCode)
            {
                try
                {
                    return await response.Content.ReadFromJsonAsync<ResponseData<List<CocktailCategory>>>(_serializerOptions);
                }
                catch (JsonException ex)
                {
                    _logger.LogError($"-----> Ошибка: {ex.Message}");
                    return ResponseData<List<CocktailCategory>>.Error($"Ошибка: {ex.Message}");
                }
            }

            _logger.LogError($"-----> Данные не получены от сервера. Error: {response.StatusCode}");
            return ResponseData<List<CocktailCategory>>.Error($"Данные не получены от сервера. Error: {response.StatusCode}");
        }
    }
}