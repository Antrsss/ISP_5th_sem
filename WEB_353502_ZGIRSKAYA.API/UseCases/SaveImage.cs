using MediatR;

namespace WEB_353502_ZGIRSKAYA.API.UseCases
{
    public sealed record SaveImage(IFormFile File) : IRequest<string>;

    public class SaveImageHandler : IRequestHandler<SaveImage, string>
    {
        private readonly IWebHostEnvironment _env;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public SaveImageHandler(
            IWebHostEnvironment env,
            IHttpContextAccessor httpContextAccessor)
        {
            _env = env;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<string> Handle(SaveImage request, CancellationToken cancellationToken)
        {
            if (request.File == null || request.File.Length == 0)
            {
                throw new ArgumentException("Файл не предоставлен или пустой");
            }

            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".bmp" };
            var fileExtension = Path.GetExtension(request.File.FileName).ToLowerInvariant();

            if (string.IsNullOrEmpty(fileExtension) || !allowedExtensions.Contains(fileExtension))
            {
                throw new ArgumentException("Недопустимый формат файла. Разрешены только изображения.");
            }

            var fileName = $"{Guid.NewGuid()}{fileExtension}";
            var uploadsFolder = Path.Combine(_env.WebRootPath, "images");

            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            var filePath = Path.Combine(uploadsFolder, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await request.File.CopyToAsync(stream, cancellationToken);
            }

            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext == null)
            {
                throw new InvalidOperationException("HttpContext недоступен");
            }

            var requestScheme = httpContext.Request.Scheme;
            var requestHost = httpContext.Request.Host;
            var imageUrl = $"{requestScheme}://{requestHost}/images/{fileName}";

            return imageUrl;
        }
    }
}
