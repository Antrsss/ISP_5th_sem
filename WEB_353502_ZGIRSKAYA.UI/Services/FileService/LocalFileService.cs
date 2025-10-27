namespace WEB_353502_ZGIRSKAYA.UI.Services.FileService
{
    public class LocalFileService(IWebHostEnvironment environment) : IFileService
    {
        public async Task<string> SaveFileAsync(IFormFile file)
        {
            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";

            var imagesPath = Path.Combine(environment.WebRootPath, "Images");
            var filePath = Path.Combine(imagesPath, fileName);

            if (!Directory.Exists(imagesPath))
            {
                Directory.CreateDirectory(imagesPath);
            }

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return Path.Combine("Images", fileName);
        }
    }
}
