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
            var urlString = new StringBuilder($"{_httpClient.BaseAddress!.AbsoluteUri}");

            // добавить категорию в маршрут
            if (!string.IsNullOrEmpty(categoryNormalizedName))
            {
                urlString.Append($"{categoryNormalizedName}/");
            }

            // добавить номер страницы в маршрут
            if (pageNo > 1)
            {
                urlString.Append($"page{pageNo}");
            }

            // добавить размер страницы в строку запроса
            if (!_pageSize.Equals("3"))
            {
                urlString.Append(QueryString.Create("pageSize", _pageSize));
            }

            // отправить запрос к API
            var response = await _httpClient.GetAsync(new Uri(urlString.ToString()));

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
                // Генерируем уникальное имя файла
                var fileName = $"{Guid.NewGuid()}{Path.GetExtension(formFile.FileName)}";
                var filePath = Path.Combine("wwwroot", "Images", fileName);

                // Сохраняем файл
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await formFile.CopyToAsync(stream);
                }

                cocktail.PathToPicture = $"Images/{fileName}";
                cocktail.MimeType = formFile.ContentType;
            }
            catch (Exception ex)
            {
                _logger.LogError($"-----> Ошибка при загрузке изображения: {ex.Message}");
                throw;
            }
        }
    }
}