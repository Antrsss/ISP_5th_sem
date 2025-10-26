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
    public class AccountController(IHttpContextAccessor contextAccessor,
                                   HttpClient httpClient,
                                   ITokenAccessor tokenAccessor,
                                   IOptions<KeycloakData> options,
                                   IFileService fileService) : Controller
    {

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
                    await tokenAccessor.SetAuthorizationHeaderAsync(httpClient, true);
                }
                catch (Exception ex)
                {
                    return Unauthorized();
                }
                var avatarUrl = "/images/default-profile-picture.png";
                // сохранить Avatar, если аватар был передан при регистрации
                if (user.Avatar != null)
                {
                    avatarUrl = await fileService.SaveFileAsync(user.Avatar);
                }
                // Подготовка данных нового пользователя
                var newUser = new CreateUserModel();
                newUser.Attributes.Add("avatar", avatarUrl);
                newUser.Email = user.Email;
                newUser.Username = user.Email;
                newUser.Credentials.Add(new UserCredentials { Value = user.Password });

                // Keycloak user endpoint
                var requestUri =
                    $"{options.Value.Host}/admin/realms/{options.Value.Realm}/users";
                // Подготовить контент запроса
                var serializerOptions = new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                };
                var userData = JsonSerializer.Serialize(newUser, serializerOptions);
                HttpContent content = new StringContent(userData, Encoding.UTF8, "application/json");
                // Отправить запрос
                var response = await httpClient.PostAsync(requestUri, content);//,serializerOptions);

                if (response.IsSuccessStatusCode)
                {
                    return Redirect(Url.Action("Index", "Home"));
                }
                else return BadRequest(response.StatusCode);
            }
            return View(user);

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
