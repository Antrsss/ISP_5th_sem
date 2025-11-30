using System.Net.Http.Json;
using System.Text;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.AspNetCore.WebUtilities;
using WEB_353502_ZGIRSKAYA.BlazorWasm.Models;
using System.Text.Json;

namespace WEB_353502_ZGIRSKAYA.BlazorWasm.Services
{
    public class DataService : IDataService
    {
        private readonly HttpClient _httpClient;
        private readonly IAccessTokenProvider _tokenProvider;
        private readonly IConfiguration _configuration;
        private readonly JsonSerializerOptions _jsonOptions;
        private readonly string _pageSize;

        public event Action? DataLoaded;

        public List<CocktailCategory> Categories { get; set; } = new();
        public List<CocktailListDto> Cocktails { get; set; } = new();
        public bool Success { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;
        public int TotalPages { get; set; }
        public int CurrentPage { get; set; }
        public CocktailCategory SelectedCategory { get; set; } = new();

        public DataService(HttpClient httpClient, IAccessTokenProvider tokenProvider, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _tokenProvider = tokenProvider;
            _configuration = configuration;
            _pageSize = _configuration.GetValue<string>("ItemsPerPage") ?? "3";
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                PropertyNameCaseInsensitive = true
            };
        }

        private async Task<bool> SetAuthorizationHeader()
        {
            try
            {
                // Запрашиваем access token
                var tokenResult = await _tokenProvider.RequestAccessToken();

                if (tokenResult.TryGetToken(out var token))
                {
                    // Добавляем токен в заголовок Authorization
                    _httpClient.DefaultRequestHeaders.Authorization =
                        new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token.Value);

                    Console.WriteLine($"JWT token obtained: {token.Value.Substring(0, Math.Min(20, token.Value.Length))}...");
                    return true;
                }
                else
                {
                    Console.WriteLine("No access token available");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting token: {ex.Message}");
                return false;
            }
        }

        public async Task GetProductListAsync(int pageNo = 1)
        {
            try
            {
                if (!await SetAuthorizationHeader())
                {
                    Success = false;
                    ErrorMessage = "Требуется аутентификация.";
                    Cocktails = new List<CocktailListDto>();
                    DataLoaded?.Invoke();
                    return;
                }

                var route = new StringBuilder("api/Cocktail/");

                // Добавляем категорию только если она выбрана
                if (SelectedCategory != null && !string.IsNullOrEmpty(SelectedCategory.NormilisedName))
                    route.Append($"{SelectedCategory.NormilisedName}/");

                var queryData = new List<KeyValuePair<string, string>>
        {
            new("pageNo", pageNo.ToString()),
        };

                if (!_pageSize.Equals("3"))
                    queryData.Add(new KeyValuePair<string, string>("pageSize", _pageSize));

                var url = QueryHelpers.AddQueryString(route.ToString(), queryData);

                Console.WriteLine($"Fetching cocktails: {url}");

                var response = await _httpClient.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var apiResponse = JsonSerializer.Deserialize<ResponseData<ListModel<CocktailListDto>>>(content, _jsonOptions);

                    if (apiResponse?.Successfull == true && apiResponse.Data != null)
                    {
                        Cocktails = apiResponse.Data.Items;
                        TotalPages = apiResponse.Data.TotalPages;
                        CurrentPage = pageNo;
                        Success = true;
                        ErrorMessage = string.Empty;
                    }
                    else
                    {
                        Cocktails = new List<CocktailListDto>();
                        Success = false;
                        ErrorMessage = apiResponse?.ErrorMessage ?? "Ошибка при получении данных";
                    }
                }
                else
                {
                    Cocktails = new List<CocktailListDto>();
                    Success = false;
                    ErrorMessage = $"Ошибка сервера: {response.StatusCode}";
                }
            }
            catch (Exception ex)
            {
                Cocktails = new List<CocktailListDto>();
                Success = false;
                ErrorMessage = $"Ошибка: {ex.Message}";
            }

            DataLoaded?.Invoke();
        }

        public async Task GetCategoryListAsync()
        {
            try
            {
                // Устанавливаем заголовок авторизации
                if (!await SetAuthorizationHeader())
                {
                    Success = false;
                    ErrorMessage = "Требуется аутентификация для получения категорий";
                    Categories = new List<CocktailCategory>();
                    DataLoaded?.Invoke();
                    return;
                }

                var response = await _httpClient.GetAsync("api/CocktailCategory");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Categories response: {content}");

                    try
                    {
                        var categories = JsonSerializer.Deserialize<List<CocktailCategory>>(content, _jsonOptions);
                        Categories = categories ?? new List<CocktailCategory>();
                        Success = true;
                        ErrorMessage = string.Empty;
                    }
                    catch (JsonException jsonEx)
                    {
                        Success = false;
                        ErrorMessage = $"Ошибка формата данных категорий: {jsonEx.Message}";
                        Categories = new List<CocktailCategory>();
                    }
                }
                else
                {
                    Success = false;
                    ErrorMessage = $"Ошибка сервера при получении категорий: {response.StatusCode}";
                    Categories = new List<CocktailCategory>();
                }
            }
            catch (AccessTokenNotAvailableException exception)
            {
                exception.Redirect();
                Success = false;
                ErrorMessage = "Требуется аутентификация";
            }
            catch (Exception ex)
            {
                Success = false;
                ErrorMessage = $"Ошибка при получении категорий: {ex.Message}";
                Categories = new List<CocktailCategory>();
            }

            DataLoaded?.Invoke();
        }

        public async Task<CocktailDetailsDto?> GetCocktailDetailsAsync(int id)
        {
            try
            {
                if (!await SetAuthorizationHeader())
                {
                    return null;
                }

                var response = await _httpClient.GetAsync($"api/Cocktail/{id}");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var apiResponse = JsonSerializer.Deserialize<ResponseData<CocktailDetailsDto>>(content, _jsonOptions);

                    if (apiResponse?.Successfull == true && apiResponse.Data != null)
                    {
                        return apiResponse.Data;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting cocktail details: {ex.Message}");
            }

            return null;
        }
    }
}