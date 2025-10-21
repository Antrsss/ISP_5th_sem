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
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                // Добавляем настройки чтобы избежать циклических ссылок
                ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles
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
            _logger.LogInformation($"=== CREATE COCKTAIL SERVICE ===");
            _logger.LogInformation($"Name: '{cocktail.Name}'");
            _logger.LogInformation($"Description: '{cocktail.Description}'");
            _logger.LogInformation($"Price: {cocktail.Price}");
            _logger.LogInformation($"Category: {cocktail.Category?.Name} (Id: {cocktail.Category?.Id})");

            cocktail.PathToPicture = "images/noimage.jpg";

            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = _httpClient.BaseAddress
            };

            var content = new MultipartFormDataContent();

            // Добавить файл изображения
            if (formFile != null)
            {
                _logger.LogInformation($"Image file: {formFile.FileName}, Size: {formFile.Length}");
                var streamContent = new StreamContent(formFile.OpenReadStream());
                content.Add(streamContent, "file", formFile.FileName);
            }

            // Создаем объект для сериализации без циклических ссылок
            var cocktailForSerialization = new
            {
                id = cocktail.Id,
                name = cocktail.Name,
                description = cocktail.Description,
                price = cocktail.Price,
                category = cocktail.Category, // Отправляем всю категорию
                pathToPicture = cocktail.PathToPicture,
                mimeType = cocktail.MimeType
            };

            // Сериализуем объект cocktail
            var cocktailJson = JsonSerializer.Serialize(cocktailForSerialization, _serializerOptions);
            _logger.LogInformation($"Serialized cocktail: {cocktailJson}");

            var cocktailContent = new StringContent(cocktailJson, Encoding.UTF8, "application/json");
            content.Add(cocktailContent, "cocktail");

            request.Content = content;

            _logger.LogInformation("Sending request to API...");
            var response = await _httpClient.SendAsync(request, CancellationToken.None);

            _logger.LogInformation($"Response status: {response.StatusCode}");

            // ВАЖНО: Добавляем логирование сырого ответа
            var responseContentString = await response.Content.ReadAsStringAsync();
            _logger.LogInformation($"=== RAW RESPONSE ===");
            _logger.LogInformation($"Response content: {responseContentString}");
            _logger.LogInformation($"=== END RAW RESPONSE ===");

            if (response.IsSuccessStatusCode)
            {
                try
                {
                    var responseData = JsonSerializer.Deserialize<ResponseData<Cocktail>>(responseContentString, _serializerOptions);

                    _logger.LogInformation($"Deserialized response - Success: {responseData?.Successfull}");
                    _logger.LogInformation($"Deserialized cocktail Name: '{responseData?.Data?.Name}'");
                    _logger.LogInformation($"Deserialized cocktail Description: '{responseData?.Data?.Description}'");
                    _logger.LogInformation($"Deserialized cocktail Price: {responseData?.Data?.Price}");
                    _logger.LogInformation($"Deserialized cocktail Category: {responseData?.Data?.Category?.Name}");

                    return responseData ?? ResponseData<Cocktail>.Error("Пустой ответ от сервера");
                }
                catch (JsonException ex)
                {
                    _logger.LogError($"Ошибка десериализации: {ex.Message}");
                    _logger.LogError($"Stack trace: {ex.StackTrace}");
                    return ResponseData<Cocktail>.Error($"Ошибка десериализации: {ex.Message}");
                }
            }

            _logger.LogError($"Object not created. Error: {response.StatusCode}");
            return ResponseData<Cocktail>.Error($"Объект не добавлен. Error: {response.StatusCode}");
        }

        public async Task UpdateCocktailAsync(int id, Cocktail cocktail, IFormFile? formFile)
        {
            _logger.LogInformation($"=== UPDATE COCKTAIL SERVICE ===");
            _logger.LogInformation($"Updating cocktail ID: {id}");
            _logger.LogInformation($"Name: '{cocktail.Name}'");
            _logger.LogInformation($"Description: '{cocktail.Description}'");
            _logger.LogInformation($"Price: {cocktail.Price}");
            _logger.LogInformation($"Category: {cocktail.Category?.Name} (Id: {cocktail.Category?.Id})");

            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Put,
                RequestUri = new Uri(_httpClient.BaseAddress!, $"{id}")
            };

            var content = new MultipartFormDataContent();

            // Добавить файл изображения
            if (formFile != null)
            {
                _logger.LogInformation($"Image file: {formFile.FileName}, Size: {formFile.Length}");
                var streamContent = new StreamContent(formFile.OpenReadStream());
                streamContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(formFile.ContentType);
                content.Add(streamContent, "file", formFile.FileName);
            }

            // Создаем объект для сериализации без циклических ссылок
            var cocktailForSerialization = new
            {
                id = cocktail.Id,
                name = cocktail.Name,
                description = cocktail.Description,
                price = cocktail.Price,
                category = cocktail.Category, // Отправляем всю категорию
                pathToPicture = cocktail.PathToPicture,
                mimeType = cocktail.MimeType
            };

            // Добавить объект cocktail
            var cocktailJson = JsonSerializer.Serialize(cocktailForSerialization, _serializerOptions);
            _logger.LogInformation($"Serialized cocktail for update: {cocktailJson}");

            var cocktailContent = new StringContent(cocktailJson, Encoding.UTF8, "application/json");
            content.Add(cocktailContent, "cocktail");

            request.Content = content;

            _logger.LogInformation("Sending update request to API...");
            var response = await _httpClient.SendAsync(request, CancellationToken.None);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError($"-----> Объект не обновлен. Error: {response.StatusCode}");
                throw new Exception($"Объект не обновлен. Error: {response.StatusCode}");
            }

            // Можно прочитать ответ если нужно
            var responseContent = await response.Content.ReadAsStringAsync();
            _logger.LogInformation($"Update response: {responseContent}");
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
    }
}