using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Text;
using System.Text.Json;
using WEB_353502_ZGIRSKAYA.UI.HelperClasses;
using WEB_353502_ZGIRSKAYA.UI.Models;
using WEB_353502_ZGIRSKAYA.UI.Services.Authentication;
using WEB_353502_ZGIRSKAYA.UI.Services.FileService;

namespace WEB_353502_ZGIRSKAYA.UI.Controllers
{
    public class AccountController : Controller
    {
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly HttpClient _httpClient;
        private readonly ITokenAccessor _tokenAccessor;
        private readonly IOptions<KeycloakData> _options;
        private readonly IFileService _fileService;

        public AccountController(IHttpContextAccessor contextAccessor,
                               HttpClient httpClient,
                               ITokenAccessor tokenAccessor,
                               IOptions<KeycloakData> options,
                               IFileService fileService)
        {
            _contextAccessor = contextAccessor;
            _httpClient = httpClient;
            _tokenAccessor = tokenAccessor;
            _options = options;
            _fileService = fileService;
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View(new RegisterUserViewModel());
        }

        [HttpPost]
        [AutoValidateAntiforgeryToken]
        public async Task<IActionResult> Register(RegisterUserViewModel user)
        {
            if (ModelState.IsValid)
            {
                if (user == null)
                {
                    return BadRequest();
                }

                try
                {
                    await _tokenAccessor.SetAuthorizationHeaderAsync(_httpClient, true);

                    var token = _httpClient.DefaultRequestHeaders.Authorization?.Parameter;

                    if (string.IsNullOrEmpty(token))
                    {
                        ModelState.AddModelError("", "Не удалось получить токен авторизации");
                        return View(user);
                    }

                    var testUrl = $"{_options.Value.Host}/admin/realms/{_options.Value.Realm}/users?max=1";

                    var testResponse = await _httpClient.GetAsync(testUrl);

                    if (!testResponse.IsSuccessStatusCode)
                    {
                        var testError = await testResponse.Content.ReadAsStringAsync();
                        ModelState.AddModelError("",
                            $"Нет прав на управление пользователями. Status: {testResponse.StatusCode}");
                        return View(user);
                    }
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Ошибка авторизации: {ex.Message}");
                    return View(user);
                }

                var avatarUrl = "/images/default-profile-picture.png";
                if (user.Avatar != null)
                {
                    avatarUrl = await _fileService.SaveFileAsync(user.Avatar);
                }

                var newUser = new CreateUserModel();
                newUser.Attributes.Add("avatar", avatarUrl);
                newUser.Email = user.Email;
                newUser.Username = user.Email;
                newUser.Credentials.Add(new UserCredentials { Value = user.Password });

                var requestUri = $"{_options.Value.Host}/admin/realms/{_options.Value.Realm}/users";

                var serializerOptions = new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                };
                var userData = JsonSerializer.Serialize(newUser, serializerOptions);
                HttpContent content = new StringContent(userData, Encoding.UTF8, "application/json");

                try
                {
                    var response = await _httpClient.PostAsync(requestUri, content);

                    if (response.IsSuccessStatusCode)
                    {
                        return Redirect(Url.Action("Index", "Home"));
                    }
                    else
                    {
                        var errorContent = await response.Content.ReadAsStringAsync();
                        ModelState.AddModelError("", $"Ошибка создания пользователя: {response.StatusCode}");
                        return View(user);
                    }
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Ошибка при создании пользователя");
                    return View(user);
                }
            }
            return View(user);
        }

        [HttpGet]
        public async Task Login()
        {
            await HttpContext.ChallengeAsync(
                "keycloak",
                new AuthenticationProperties
                {
                    RedirectUri = Url.Action("Index", "Home")
                });
        }

        [HttpPost]
        public async Task Logout()
        {
            await
                HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignOutAsync("keycloak",
            new AuthenticationProperties
            {
                RedirectUri = Url.Action("Index", "Home")
            });
        }

        class UserCredentials
        {
            public string Type { get; set; } = "password";
            public bool Temporary { get; set; } = false;
            public string Value { get; set; }
        }

        class CreateUserModel
        {
            public Dictionary<string, string> Attributes { get; set; } = new();
            public string Username { get; set; }
            public string Email { get; set; }
            public bool Enabled { get; set; } = true;
            public bool EmailVerified { get; set; } = true;
            public List<UserCredentials> Credentials { get; set; } = new();
        }
    }
}