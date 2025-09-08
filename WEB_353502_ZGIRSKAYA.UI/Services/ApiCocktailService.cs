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
        private readonly ILogger<ApiCocktailService> _logger;

        public ApiCocktailService(HttpClient httpClient, IConfiguration configuration, ILogger<ApiCocktailService> logger)
        {
            _httpClient = httpClient;
            _pageSize = configuration.GetValue<string>("ItemsPerPage") ?? "3";
            _serializerOptions = new JsonSerializerOptions()
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
            _logger = logger;
        }

        public async Task<ResponseData<ListModel<Cocktail>>> GetCocktailListAsync(string? categoryNormalizedName, int pageNo = 1)
        {
            // подготовка URL запроса
            var queryParams = new List<string>();

            // добавить категорию в маршрут
            var urlString = string.IsNullOrEmpty(categoryNormalizedName)
                ? $"{_httpClient.BaseAddress!.AbsoluteUri}"
                : $"{_httpClient.BaseAddress!.AbsoluteUri}{categoryNormalizedName}";

            // добавить параметры в query string
            if (pageNo > 1)
            {
                queryParams.Add($"pageNo={pageNo}");
            }

            if (!_pageSize.Equals("3"))
            {
                queryParams.Add($"pageSize={_pageSize}");
            }

            // добавить query string если есть параметры
            if (queryParams.Any())
            {
                urlString += "?" + string.Join("&", queryParams);
            }

            // отправить запрос к API
            var response = await _httpClient.GetAsync(new Uri(urlString));

            if (response.IsSuccessStatusCode)
            {
                try
                {
                    return await response.Content.ReadFromJsonAsync<ResponseData<ListModel<Cocktail>>>(_serializerOptions);
                }
                catch (JsonException ex)
                {
                    _logger.LogError($"-----> Ошибка: {ex.Message}");
                    return ResponseData<ListModel<Cocktail>>.Error($"Ошибка: {ex.Message}");
                }
            }

            _logger.LogError($"-----> Данные не получены от сервера. Error: {response.StatusCode}");
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
                    _logger.LogError($"-----> Ошибка: {ex.Message}");
                    return ResponseData<Cocktail>.Error($"Ошибка: {ex.Message}");
                }
            }

            _logger.LogError($"-----> Данные не получены от сервера. Error: {response.StatusCode}");
            return ResponseData<Cocktail>.Error($"Данные не получены от сервера. Error: {response.StatusCode}");
        }

        public async Task<ResponseData<Cocktail>> CreateCocktailAsync(Cocktail cocktail, IFormFile? formFile)
        {
            try
            {
                // Если есть файл изображения, загружаем его
                if (formFile != null)
                {
                    await UploadImageAsync(cocktail, formFile);
                }
                else
                {
                    cocktail.PathToPicture = "Images/noimage.jpg";
                }

                var content = new StringContent(JsonSerializer.Serialize(cocktail, _serializerOptions), Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync("", content);

                if (response.IsSuccessStatusCode)
                {
                    try
                    {
                        var responseData = await response.Content.ReadFromJsonAsync<ResponseData<Cocktail>>(_serializerOptions);
                        return responseData;
                    }
                    catch (JsonException ex)
                    {
                        _logger.LogError($"-----> Ошибка: {ex.Message}");
                        return ResponseData<Cocktail>.Error($"Ошибка: {ex.Message}");
                    }
                }

                _logger.LogError($"-----> Объект не создан. Error: {response.StatusCode}");
                return ResponseData<Cocktail>.Error($"Объект не создан. Error: {response.StatusCode}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"-----> Ошибка при создании коктейля: {ex.Message}");
                return ResponseData<Cocktail>.Error($"Ошибка при создании коктейля: {ex.Message}");
            }
        }

        public async Task UpdateCocktailAsync(int id, Cocktail cocktail, IFormFile? formFile)
        {
            try
            {
                // Если есть файл изображения, загружаем его
                if (formFile != null)
                {
                    await UploadImageAsync(cocktail, formFile);
                }

                var content = new StringContent(JsonSerializer.Serialize(cocktail, _serializerOptions), Encoding.UTF8, "application/json");
                var response = await _httpClient.PutAsync($"{id}", content);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError($"-----> Объект не обновлен. Error: {response.StatusCode}");
                    throw new Exception($"Объект не обновлен. Error: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"-----> Ошибка при обновлении коктейля: {ex.Message}");
                throw;
            }
        }

        public async Task DeleteCocktailAsync(int id)
        {
            var response = await _httpClient.DeleteAsync($"{id}");

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError($"-----> Объект не удален. Error: {response.StatusCode}");
                throw new Exception($"Объект не удален. Error: {response.StatusCode}");
            }
        }

        private async Task UploadImageAsync(Cocktail cocktail, IFormFile formFile)
        {
            try
            {
                // Используем MultipartFormDataContent для отправки файла
                using var content = new MultipartFormDataContent();
                using var fileStream = formFile.OpenReadStream();
                var fileContent = new StreamContent(fileStream);
                fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(formFile.ContentType);

                content.Add(fileContent, "file", formFile.FileName);

                // Отправляем файл на API
                var response = await _httpClient.PostAsync("upload", content);

                if (response.IsSuccessStatusCode)
                {
                    var uploadedPath = await response.Content.ReadAsStringAsync();
                    cocktail.PathToPicture = uploadedPath.Trim('"'); // Убираем кавычки если есть
                    cocktail.MimeType = formFile.ContentType;
                    _logger.LogInformation($"Изображение загружено: {cocktail.PathToPicture}");
                }
                else
                {
                    _logger.LogError($"Ошибка загрузки изображения: {response.StatusCode}");
                    cocktail.PathToPicture = "Images/noimage.jpg"; // Fallback
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Ошибка при загрузке изображения: {ex.Message}");
                cocktail.PathToPicture = "Images/noimage.jpg"; // Fallback
            }
        }
    }
}