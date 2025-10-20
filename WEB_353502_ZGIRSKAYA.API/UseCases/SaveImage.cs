using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace WEB_353502_ZGIRSKAYA.API.UseCases
{
    public sealed record SaveImage(IFormFile file) : IRequest<string>;
    public class SaveImageHandler : IRequestHandler<SaveImage, string>
    {
        private readonly IWebHostEnvironment _env;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public SaveImageHandler(IWebHostEnvironment env, IHttpContextAccessor httpContextAccessor)
        {
            _env = env;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<string> Handle(SaveImage request, CancellationToken cancellationToken)
        {
            if (request.file == null || request.file.Length == 0)
                return null;

            // Уникальное имя файла
            var fileName = Guid.NewGuid() + Path.GetExtension(request.file.FileName);

            // Папка wwwroot/images
            var uploadPath = Path.Combine(_env.WebRootPath, "images");
            if (!Directory.Exists(uploadPath))
                Directory.CreateDirectory(uploadPath);

            // Полный путь к файлу
            var filePath = Path.Combine(uploadPath, fileName);

            // Сохраняем файл
            await using var stream = new FileStream(filePath, FileMode.Create);
            await request.file.CopyToAsync(stream, cancellationToken);

            // Формируем URL
            var requestScheme = _httpContextAccessor.HttpContext.Request.Scheme;
            var requestHost = _httpContextAccessor.HttpContext.Request.Host;
            var fileUrl = $"{requestScheme}://{requestHost}/images/{fileName}";

            return fileUrl;
        }
    }
}
